/// <reference path="./typings/main.d.ts" />

'use strict'

var http = require('http');
var fs = require('fs');
var url = require('url');
var path = require('path');
var chalk = require('chalk');
var querystring = require('querystring');
var host = process.argv[2] || "localhost";
var port = parseInt(process.argv[3], 10) || 6969;

var server = http.createServer(function(request, response) {

    let uri = url.parse(request.url).pathname;
    let filename = path.join(process.cwd(), uri);

    var date = new Date();
    var timestamp = `${date.getDay()}.${date.getMonth()}. ${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`;

    console.log(`[${timestamp}] ` + chalk.blue(`${request.method} `) + request.url);

    if (request.url === '/index' || request.url === '/admin') filename += '.html';

    if (request.method === 'GET') {

        if (request.headers.accept.indexOf('application/json') !== -1 && filename.split('.').reverse()[0] !== 'json') filename += '.json';

        fs.exists(filename, exists => {
            if (!exists) {
                response.writeHead(404, { "Content-Type": "text/plain" });
                response.write("404 Not Found\n");
                response.end();
                return;
            }

            if (fs.statSync(filename).isDirectory()) filename += 'index.html';


            fs.readFile(filename, "binary", (err, file) => {
                if (err) {
                    response.writeHead(500, { "Content-Type": "text/plain" });
                    response.write(err + "\n");
                    response.end();
                    return;
                }
                response.writeHead(200, getFileMime(filename));
                response.write(file, "binary");
                response.end();
            });
        });
    }

    if (request.method === 'POST') {

        let fullBody = '';

        request.on('data', chunk => fullBody += chunk.toString());

        request.on('end', () => {

            if (filename.split('.').reverse()[0] !== 'json') filename += '.json';
            let obj = [];
            fs.exists(filename, exists => {
                if (exists) {
                    obj = JSON.parse(fs.readFileSync(filename));
                }
                let decodedBody = querystring.parse(fullBody);

                decodedBody.id = (Number(obj.reverse()[0].id) + 1);

                obj.push(decodedBody);


                fs.writeFile(filename, JSON.stringify(obj), err => {
                    if (err) throw err;
                    response.writeHead(200, "OK", { 'Content-Type': 'application/json' });
                    response.write(JSON.stringify(decodedBody));
                    response.end();
                });
            });
        });
    }

    if (request.method === 'DELETE') {
        let contentId = Number(request.url.split('/').reverse()[0]);
        filename = filename.split('\\');
        filename.pop();
        filename = filename.join('\\');

        if (filename.split('.').reverse()[0] !== 'json') filename += '.json';

        fs.exists(filename, exists => {
            if (!exists) {
                response.writeHead(404, "File not found", { 'Content-Type': 'text/plain' });
                response.end();
                return;
            }
            let obj = [].concat(JSON.parse(fs.readFileSync(filename)));

            if (contentId === NaN) {
                response.writeHead(400, "ID malformed", { 'Content-Type': 'text/plain' });
                response.end();
                return;
            }

            let temp = obj.filter(p => p.id === contentId);

            if (temp.length === 0) {
                response.writeHead(400, `Id not found: ${contentId}`, { 'Content-Type': 'text/plain' });
                response.end();
                return;
            }

            obj.splice(obj.indexOf(temp[0]), 1);

            fs.writeFile(filename, JSON.stringify(obj), err => {
                if (err) throw err;
                response.writeHead(200, "OK", { 'Content-Type': 'application/json' });
                response.write(String(contentId));
                response.end();
            });

        });
    }

    if (request.method === 'PUT') {
        let fullBody = '';

        request.on('data', chunk => fullBody += chunk.toString());

        request.on('end', () => {

            let contentId = Number(request.url.split('/').reverse()[0]);
            filename = filename.split('\\');
            filename.pop();
            filename = filename.join('\\');

            if (filename.split('.').reverse()[0] !== 'json') filename += '.json';

            fs.exists(filename, exists => {
                if (!exists) {
                    response.writeHead(404, "File not found", { 'Content-Type': 'text/plain' });
                    response.end();
                    return;
                }

                if (contentId === NaN) {
                    response.writeHead(400, "ID malformed", { 'Content-Type': 'text/plain' });
                    response.end();
                    return;
                }

                let decodedBody = querystring.parse(fullBody);
                let obj = [].concat(JSON.parse(fs.readFileSync(filename)));

                let temp = obj.filter(p => p.id === contentId);

                if (temp.length === 0) {
                    response.writeHead(400, `Id not found: ${contentId}`, { 'Content-Type': 'text/plain' });
                    response.end();
                    return;
                }

                decodedBody.id = temp[0].id;

                obj.splice(obj.indexOf(temp[0]), 1, decodedBody);

                fs.writeFile(filename, JSON.stringify(obj), err => {
                    if (err) throw err;
                    response.writeHead(200, "OK", { 'Content-Type': 'application/json' });
                    response.write(JSON.stringify(decodedBody));
                    response.end();
                });
            });
        });
    }
});

console.log(`\nStarting server...`);
server.listen(port, host, () => {
    console.log(`${chalk.cyan("Serving ")} ${chalk.magenta(process.cwd())} at ${chalk.hidden.white(`http://${host}:${port}/`)}`);
});

function getFileMime(filename) {
    switch (filename.split('.').reverse()[0]) {
        case 'html':
            return 'text/html';
        case 'js':
            return 'application/javascript';
        case 'css':
            return 'text/css';
        case 'png':
            return 'image/png';
        default:
            return 'text/plain';
    }
}