var myApp = angular.module('myApp', []);

//myApp.config(function ($routeProvider) {

//	$routeProvider
//		.when("/Upload/Upload", {
//			templateUrl: "Upload.cshtml"
//		})
//		.when("/NavigationBar/MyVideos", {
//			templateUrl: "MyVideos.cshtml"
//		})
//})

myApp.controller('registrationCtrl', function ($scope, $http, $window) {



    $scope.certuser = {};
    var status = {};

    $scope.savedata = function (certuser) {


        $http.post("/Registration/SaveUser", { newCertUser: certuser })
            .then(function succesCallback(response) {
                status = response.data;
                if (status === 0) {
                    alert("Registration successfull");
                    $window.location.href = "/Login/Index";
                }
                else if (status === 1) {
                    alert("Username is already in use");
                }
                else if (status === 2) {
                    alert("Email is already in use");
                }
                else if (status === 3) {
                    alert("Sending mail error");
                }



            }, function errorCallback(response) {
                alert("POST method error");
            });

    };

});

myApp.controller('uploadController', function ($scope, $http, $window) {

    $scope.video = {};
	var status = {};

    var videoTitle = "";

    $scope.priceRegEx = /^(\d*\.)?\d+$/;
    $scope.regex = /^(?:https?:\/\/)?(?:www\.)?(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=))((\w|-){11})(?:\S+)?$/;


    var tag = document.createElement('script');

    tag.src = "https://www.youtube.com/iframe_api";
    var firstScriptTag = document.getElementsByTagName('script')[0];
    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);


    window.onYouTubeIframeAPIReady = function () {
        $scope.player = new YT.Player('player', {

            videoId: youtube_parser(videoTitle),
            events: {
                'onReady': onPlayerReady,
                'onStateChange': onPlayerStateChange
            },
            playerVars: {
                'controls': 0,
                'showinfo': 0
            }
        });


        function onPlayerReady(event) {
            $scope.player.playVideo();
        }

        function youtube_parser(url) {
            var regExp = /^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#\&\?]*).*/i;
            var match = url.match(regExp);
            return (match && match[7].length === 11) ? match[7] : false;
        }

        var firstLoad = true;

        //	// The API calls this function when the player's state changes.
        function onPlayerStateChange(event) {
            if (firstLoad === true && event.data === -1) {
                console.log(firstLoad);
                firstLoad = false;
            }
        }
    };

    //	$scope.set2 = function ($ayd) {
    //		var thumb = getParameterByName(this.input.ayd, 'v'),
    //			url = 'http://img.youtube.com/vi/' + thumb + '/default.jpg';
    //		this.thumb = url
    //	}

    //	function getParameterByName(url, name) {
    //		name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    //		var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
    //			results = regex.exec(url);
    //		return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    //	}
    //}


    $scope.check = function (filepath) {

        $scope.player.loadVideoById(youtube_parser(filepath));

        $scope.player.stopVideo();

        function youtube_parser(url) {
            var regExp = /^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#\&\?]*).*/i;
            var match = url.match(regExp);
            return (match && match[7].length === 11) ? match[7] : false;
        }
    };

	
    $scope.savevideo = function (video) {

        $http.post("/Upload/SaveVideo", { newVideo: video })
            .then(function succesCallback(response) {
                status = response.data;
				if (status === 0) {
					alert("Upload successfull");
					$window.location.href = '/NavigationBar/MyVideos';
					$scope.video = "";
				}
				else if (status === 1) {
					alert("You already upload video with that URL");
				}
				else if (status === 2) {
					alert("There is already video with the same URL in system");
				}
				
            }, function errorCallback(response) {
                alert("Invalid input!");
            });
    };
});

