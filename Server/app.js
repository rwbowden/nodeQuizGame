var io = require('socket.io')(process.envPort||5000);
var http = require('http');
var shortid = require('shortid');
var path = require('path');
var logger = require('morgan');
var bodyParser = require('body-parser');
var MongoClient = require('mongodb').MongoClient;
var express = require('express');
var util = require('util');
var url = "mongodb://localhost:27017/";

console.log("Server Started");
//console.log(shortid.generate());

var questions = [];

var dbObj;

var app = express(); 

app.set('views', path.resolve(__dirname, 'views'));
app.set('view engine', 'ejs');

app.use(logger("dev"));
app.use(bodyParser.urlencoded({extended:true}));

MongoClient.connect(url, function(err, client){
    if(err) throw err;
    dbObj = client.db("SocketGameData");
    console.log("Connected to MongoDB");
});


app.get("/", function(request,response){
        
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
            .then(function(data){

                var newData = {
                    rounds : data.allRounds,
                    param : request.query.num
                }

                response.render("index", {newData});
            });
        //response.render("index");


});

app.get("/new-entry", function(request,response){
        
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
            .then(function(data){

                var newData = {
                    param : request.query.num,
                    rounds : data.allRounds[request.query.num]
                }

                response.render("new-entry", {newData});
            });
        //response.render("index");

});

app.post("/new-entry", function(request,response){

    dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
    .then(function(data){

        //console.log(data);

        var newData = {
            param : request.query.num,
            rounds : data.allRounds[request.query.num],
            name : data.allRounds[request.query.num].name
        }
        
        //console.log("Old: " + JSON.stringify(newData.name));
        
        for(var i = 0; i < newData.rounds.questions.length; i++){

            var question =  newData.rounds.questions[i];

            if(i > 1)
                var text = request.body.question[i];
            else
                var text = request.body.question;

            newData.rounds.questions[i].questionText = text;
            


            for(var a = 0; a < question.answers.length; a++)
            {
                console.log(util.inspect(request.body.isCorrect[a]));
                question.answers[a].answerText = request.body.answer[a];
                if(request.body.isCorrect[a] == "n")
                    question.answers[a].isCorrect = false;
                else
                    question.answers[a].isCorrect = true;
                //question.answers[a].isCorrect = Boolean(request.body.isCorrect[a]);
            }

            //newData.rounds.questions[i].answers = request.body.answers;
        }

                dbObj.collection("playerData").findOneAndUpdate(
                {}, 
                {"$set" : {"allRounds.$[elem]" : newData.rounds} },
                {sort:{$natural:-1},
                    arrayFilters: [{"elem.name": {$gte: newData.name}}]
                }
                ).then(function(data){
                    console.log(data)
                });
        return data;

    });

    //console.log(newData.param);
   

    response.redirect("/");
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