var io = require('socket.io')(process.envPort||5000);
var http = require('http');
var path = require('path');
var logger = require('morgan');
var bodyParser = require('body-parser');
var MongoClient = require('mongodb').MongoClient;
var express = require('express');
var cookieParser = require('cookie-parser');
var passport = require('passport');
var session = require('express-session');
var url = "mongodb://robbie:testpass@ds115219.mlab.com:15219/gamedata";

console.log("Server Started");
//console.log(shortid.generate());

var questions = [];

var dbObj;

var app = express(); 

app.set('views', path.resolve(__dirname, 'views'));
app.set('view engine', 'ejs');

app.use(logger("dev"));
app.use(bodyParser.urlencoded({extended:true}));

app.use(cookieParser());
app.use(session({
	secret:"secretSession",
	resave:true,
	saveUninitialized:true
}));

app.use(passport.initialize());
app.use(passport.session());

MongoClient.connect(url, function(err, client){
    if(err) throw err;
    dbObj = client.db("gamedata");
    console.log("Connected to MongoDB");
});

passport.serializeUser(function(user, done){
	done(null, user);
});

passport.deserializeUser(function(user, done){
	done(null, user);
});

var userHost;

LocalStrategy = require('passport-local').Strategy;
passport.use(new LocalStrategy({
	usernameField:'',
	passwordField:''
	},
	function(username, password, done){

			dbObj.collection("users").findOne({username:username}, function(err, results){
				if(err) throw err;
				if(results){
					if(results.password === password){
					var user = results;
					userHost = user;
					done(null, user);
				}
				else{
					done(null, false,{mesage:'Bad Password'});
				}
			}
			else done(null, false, {mesage: 'User not found'});
			});

	}
	));

var loggedIn = false;

function ensureAuthenticated(req, res, next){
	if(req.isAuthenticated()){
		next();
	}
	else{
		res.redirect("/sign-up");
	}
}

app.get("/", function(request,response){
        
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
            .then(function(data){

                var newData = {
                    rounds : data.allRounds,
                    param : request.query.num,
                    in : loggedIn
                }

                response.render("index", {newData});
            });
});

app.get("/high-scores", function(request, response){
    dbObj.collection("scores").find().sort({'score' : -1}).limit(10).toArray().then(function(data){

        var newData = {
            in : loggedIn,
            scores : data
        }

        response.render("high-scores", {newData});
    });
});

app.get("/new-entry", ensureAuthenticated,function(request,response){
        
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
            .then(function(data){

                var newData = {
                    param : request.query.num,
                    rounds : data.allRounds[request.query.num],
                    in : loggedIn
                }

                response.render("new-entry", {newData});
            });

});

app.get("/sign-in", function(request,response){
    var newData = {
        in : loggedIn,
        hasPassed : pass
    }

    pass = false;
	response.render("sign-in", {newData});
});

var taken = false;
app.get("/sign-up", function(request,response){
    var newData = {
        in : loggedIn,
        isTaken : taken
    }
    taken = false;
	response.render("sign-up", {newData});
});

app.get('/logout', function(req, res){
    req.logout();
    loggedIn = false;
	res.redirect("/");
});

app.post("/new-entry", function(request,response){
    if(!request.body.isCorrect || request.body.isCorrect.length != (1 * request.body.question.length)){
        response.status(400).send("Must have exactly 1 correct answer per question!");
        return;
    }
    
    // Find newest entry in database
    dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
    .then(function(data){

        // Grab info from the round you are editing
        var newData = {
            param : request.query.num,
            rounds : data.allRounds[request.query.num],
            name : data.allRounds[request.query.num].name
        }

        // Set time and points to new time and points
        newData.rounds.timeLimitInSeconds = request.body.time;
        newData.rounds.pointsAddedForCorrectAnswer = request.body.points;

        var counter = 0; // Counter keeps track of how many answer entries you've looked at so far.
        // For each question in round
        for(var i = 0; i < newData.rounds.questions.length; i++){

            var question =  newData.rounds.questions[i];

            // If there is more than one question in round
            if(newData.rounds.questions.length > 1)
                var text = request.body.question[i]; // Use index
            else
                var text = request.body.question; // Else use only question avalible

            newData.rounds.questions[i].questionText = text; // Set question text to new question text

            // For each answer in the current question
            for(var a = counter; a < question.answers.length + counter; a++) // Start with counter value to skip checkboxes already used
            {
                // Set answer text to new answer text
                newData.rounds.questions[i].answers[a - counter].answerText = request.body.answer[a];

                // If only one question
                if( newData.rounds.questions.length == 1){
                    if(request.body.isCorrect == a) // Check if the correct answer is the current answer
                    newData.rounds.questions[i].answers[a - counter].isCorrect = true;
                    else
                    newData.rounds.questions[i].answers[a - counter].isCorrect = false;
                }
                else{
                    if(request.body.isCorrect[i] == a - counter) // Check if correctAnswer at index is the current answer
                    newData.rounds.questions[i].answers[a - counter].isCorrect = true;
                    else
                    newData.rounds.questions[i].answers[a - counter].isCorrect = false; 
                }
            }
            counter += question.answers.length; // Update counter value
        }
                //Find round with same name in newest db entry and update round to newData.rounds
                dbObj.collection("playerData").findOneAndUpdate(
                {"allRounds.name" : newData.name}, 
                {"$set" : {"allRounds.$" : newData.rounds} },
                {sort:{$natural:-1}
                    
                }
                ).then(function(data){
                    response.redirect("/"); // Load index
                });
        return data;

    });
    
});

var pass = false;
app.post("/sign-up", function(request, response){		
		var user = {
			username: request.body.username,
			password: request.body.password
        }; 

        dbObj.collection("users").findOne({username:user.username},function(err, data){

            if(!data){
                dbObj.collection("users").insert(user, function(err, results){
                    taken = false;
                    pass = true;
                    request.login(request.body, function(){
                        response.redirect('/sign-in');
                    });
                });
            }else{
                taken = true;
                pass = false;

                response.redirect('/sign-up');
            }

        });
		
});

app.post("/sign-in", passport.authenticate('local', {
	failureRedirect:'/sign-in'
	}), function(request, response){
            loggedIn = true;
			response.redirect('/');
});
    
http.createServer(app).listen(3000, function(){
	console.log("Game library server started on port 3000");
});

io.on('connection', function(socket){
    console.log("Connected");

    socket.on('Load', function()
    {
        console.log("Loading");
    });

    socket.on('get data', function(){
        console.log("Getting data");
        dbObj.collection("playerData").findOne({}, {sort:{$natural:-1}})
        .then(function(gameData){
        
        var newData = {
                game : gameData,
                score: null
            }
            
        dbObj.collection("scores").find().sort({'score' : -1}).limit(10).toArray().then(function(data){
                var dataObjs = JSON.stringify(data);
                //socket.emit('scores', dataObjs);
        
                //console.log(dataObjs);

                newData.score = data;
            }).then(function(){
                socket.emit('loaded', newData);
                console.log(newData.score);
            });

            

            
        });       
    });

    socket.on('send data', function(data){
        console.log(JSON.stringify(data));

        dbObj.collection("playerData").save(data, function(err, res){
            if(err) throw err;
            console.log("data saved to MongoDB");
        });
    });

    socket.on('send score', function(data){
        dbObj.collection("scores").save(data, function(err, res){
            if(err) throw err;
            console.log("score save to MongoDB");
        });
    });

    socket.on('get scores', function(){
        console.log("Getting scores");
        dbObj.collection("scores").find().sort({'score' : 1}).limit(10).toArray().then(function(data){

            var dataObjs = JSON.stringify(data);
            socket.emit('scores', dataObjs);
        
            console.log(dataObjs);

        });       
    });
    
});