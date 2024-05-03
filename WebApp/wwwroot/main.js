import {dotnet} from './_framework/dotnet.js'

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
    terminal.open(document.getElementById('terminal'));
    return terminal;
}

const monoFont = "JetBrains Mono";

const [{setModuleImports, getAssemblyExports, getConfig}] = await Promise.all([loadDotNET(), loadFont(monoFont)]);
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

const terminal = initializeTerminal();

setModuleImports('main.js', {
    terminal: {
        write: text => terminal.write(text),
        width: () => terminal.cols,
        height: () => terminal.rows,
        clear: () => {
            //terminal.write("\n\n\r");
            terminal.reset();
        },
    },
});

terminal.onKey(e => {
    exports.TerminalInterop.TerminalKey(e.key, e.domEvent.keyCode);
});

await dotnet.run();