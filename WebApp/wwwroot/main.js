import {dotnet} from './_framework/dotnet.js'

const monoFont = "JetBrains Mono";

function loadFont(fontFamily) {
    return new FontFaceObserver(fontFamily).load();
}

function loadDotNET() {
    return dotnet
        .withDiagnosticTracing(false)
        .withApplicationArgumentsFromQuery()
        .create();
}

function initializeTerminal() {
    const terminal = new Terminal({fontFamily: monoFont});
    terminal.open(document.getElementById("terminal"));
    
    // remove loading indicator element
    const loadingIndicator = document.getElementById("loading-indicator");
    if (loadingIndicator !== null) loadingIndicator.remove();
    
    return terminal;
}

// wait until dotnet runtime and WebApp module are loaded, and the monospace font has become available.
const [{setModuleImports, getAssemblyExports, getConfig}] = await Promise.all([loadDotNET(), loadFont(monoFont)]);
const config = getConfig();
const {WebApp} = await getAssemblyExports(config.mainAssemblyName);

const terminal = initializeTerminal();

setModuleImports('main.js', {
    terminal: {
        write: text => terminal.write(text),
        width: () => terminal.cols,
        height: () => terminal.rows,
        clear: () => terminal.reset(),
    },
    fs: {
        read: path => localStorage.getItem(path),
        write: (path, value) => localStorage.setItem(path, value),
    }
});

terminal.onKey(e => {
    WebApp.JsInterop.TerminalKey(e.key, e.domEvent.keyCode);
});

await dotnet.run();