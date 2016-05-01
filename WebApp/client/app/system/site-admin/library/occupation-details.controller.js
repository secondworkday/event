(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('OccupationDetailsController', OccupationDetailsController);

    OccupationDetailsController.$inject = ['$scope', '$log', 'FileUploader', 'siteService', 'occupation'];

    /* @ngInject */
    function OccupationDetailsController($scope, $log, FileUploader, siteService, occupation) {
      $log.debug("Loading OccupationDetailsController...");

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

    }

})();
