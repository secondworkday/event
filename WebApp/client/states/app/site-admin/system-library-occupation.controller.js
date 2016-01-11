app.controller('SystemLibraryOccupationController', function ($scope, $log, FileUploader, utilityService, siteService, occupation) {
  $log.debug("Loading SystemLibraryOccupationController...");

  $scope.occupation = occupation;

  $scope.onFocusPointUpdated = function (x, y) {
    siteService.setOccupationHeroImageFocalPoint(occupation, x, y);
  };

  $scope.setTitle = function (title) {
    siteService.setOccupationTitle(occupation, title);
  };

  $scope.setDescription = function (description) {
    siteService.setOccupationDescription(occupation, description);
  };

  $scope.onChangeReleaseState = function () {
    var releaseState = $scope.occupation.releaseState;
    siteService.setOccupationReleaseState(occupation, releaseState);
  };

  $scope.setImageLicense = function (imageLicense) {
    siteService.setOccupationHeroImageLicense(occupation, imageLicense);
  };

  $scope.setImageSource = function (imageSource) {
    siteService.setOccupationHeroImageSource(occupation, imageSource);
  };



  $scope.uploader = new FileUploader({
    url: '/upload.ashx',
    autoUpload: true,
    removeAfterUpload: true,
    formData: [{ type: 'occupationHeroImage', code: $scope.occupation.onetCode }]
  });

});


// http://stackoverflow.com/questions/13781685/angularjs-ng-src-equivalent-for-background-imageurl
app.directive('backImg', function () {
  return function (scope, element, attrs) {
    var url = attrs.backImg;
    element.css({
      'background-image': 'url(' + url + ')',
      'background-size': 'cover'
    });
  };
});

// http://kmturley.blogspot.com/2015/01/responsive-and-adaptive-images-using.html
app.directive('focusPoint', function ($parse) {
  'use strict';

  return {
    restrict: 'A',
    replace: true,
    scope: {
      model: '=ngModel'
    },
    template: '<div class="focus-point">' +
    '<div class="focus-area">' +
    '<span class="target" style="left: {{ x }}%; top: {{ y }}%"></span>' +
    '<img src="{{ src }}" alt="" ng-mousemove="onMouseMove($event)" ng-mousedown="onMouseDown($event)" ng-mouseup="onMouseUp($event)" draggable="false" class="source" />' +
    '</div>' +
    '</div>',
    link: function (scope, element, attr) {
      var dragging = false;
      scope.src = attr.src;
      var expression = attr.focusPoint || 'true';

      // Required: Parameter that provides our Search() function
      var focusPointParse = $parse(attr.focusPoint);
      //!! this isn't the best as we're assuming we're passed a function in our parent's scope.
      //   Is the ng-click way of doing things better?
      scope.focusPointChangedHandler = focusPointParse(scope.$parent);


      scope.onMouseDown = function (e) {
        scope.update(e);
        dragging = true;
      };

      scope.onMouseMove = function (e) {
        if (dragging === true) {
          scope.update(e);
        }
      };

      scope.onMouseUp = function (e) {
        e.preventDefault();
        dragging = false;
      };

      scope.update = function (e) {
        e.preventDefault();
        var offset = scope.offset(e.target);
        scope.x = Math.round(((e.pageX - offset.left) / e.target.clientWidth) * 100);
        scope.y = Math.round(((e.pageY - offset.top) / e.target.clientHeight) * 100);

        scope.focusPointChangedHandler(scope.x, scope.y);
      };

      scope.offset = function (elm) {
        try { return elm.offset(); } catch (e) { }
        var body = document.documentElement || document.body;
        return {
          left: elm.getBoundingClientRect().left + (window.pageXOffset || body.scrollLeft),
          top: elm.getBoundingClientRect().top + (window.pageYOffset || body.scrollTop)
        };
      };
    }
  };
});
