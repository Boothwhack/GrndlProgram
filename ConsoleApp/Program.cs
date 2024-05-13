using Application;
using ConsoleUI;

var terminal = new ConsoleTerminal();
var fs = new FileFileSystem();
var app = new Application.Application(terminal, fs);
await app.Run();
