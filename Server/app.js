var io = require('socket.io')(process.envPort||5000);
var http = require('http');
var shortid = require('shortid');
var path = require('path');
var logger = require('morgan');
var MongoClient = require('mongodb').MongoClient;
var express = require('express');
var url = "mongodb://localhost:27017/";

console.log("Server Started");
//console.log(shortid.generate());

var questions = [];

var dbObj;

var app = express(); 

app.set('views', path.resolve(__dirname, 'views'));
app.set('view engine', 'ejs');

app.use(logger("dev"));

MongoClient.connect(url, function(err, client){
    if(err) throw err;
    dbObj = client.db("SocketGameData");
    console.log("Connected to MongoDB");
});


app.get("/", function(request,response){
	MongoClient.connect(url, function(err, db){
		if(err) throw err;
        console.log("here");
        
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
            .then(function(data){
                console.log(data.allRounds);
                var movies = data.allRounds;
                response.render("index", {movies});
            });
        //response.render("index");

	});
});

http.createServer(app).listen(3000, function(){
	console.log("Game library server started on port 3000");
});

io.on('connection', function(socket){
    var thisPlayerId = shortid.generate();
    console.log("Connected");

    socket.on('Load', function()
    {
        console.log("Loading");
    });

    socket.on('get data', function(){
        console.log("Getting data");
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
        .then(function(gameData){
            socket.emit('loaded', gameData);
            console.log(gameData);
        });       
    });

    socket.on('send data', function(data){
        console.log(JSON.stringify(data));

        dbObj.collection("playerData").save(data, function(err, res){
            if(err) throw err;
            console.log("data saved to MongoDB");
        });
    });
});