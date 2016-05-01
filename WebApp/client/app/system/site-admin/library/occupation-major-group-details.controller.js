(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('OccupationMajorGroupDetailsController', OccupationMajorGroupDetailsController);

    OccupationMajorGroupDetailsController.$inject = ['$scope', '$log', 'FileUploader', 'siteService', 'occupationMajorGroup'];

    /* @ngInject */
    function OccupationMajorGroupDetailsController($scope, $log, FileUploader, siteService, occupationMajorGroup) {
      $log.debug("Loading OccupationMajorGroupDetailsController...");

      $scope.occupationMajorGroup = occupationMajorGroup;

      $scope.onFocusPointUpdated = function (x, y) {
        siteService.setOccupationHeroImageFocalPoint(occupationMajorGroup, x, y)
        .then(function () {
          // refresh our object since URLs should have changed - and this object doesn't get notifications
          // (don't need to do anything with the result as fresh data will be added to our existing $scope.occupationMajorGroup object)
          siteService.getOccupationAuxiliaryData(occupationMajorGroup.onetCode);
        });
      };

      $scope.onChangeReleaseState = function () {
        var releaseState = $scope.occupationMajorGroup.releaseState;
        siteService.setOccupationReleaseState(occupationMajorGroup, releaseState);
      };

      $scope.setImageLicense = function (imageLicense) {
        siteService.setOccupationHeroImageLicense(occupationMajorGroup, imageLicense);
      };

      $scope.setImageSource = function (imageSource) {
        siteService.setOccupationHeroImageSource(occupationMajorGroup, imageSource);
      };


      $scope.uploader = new FileUploader({
        url: '/upload.ashx',
        autoUpload: true,
        removeAfterUpload: true,
        formData: [{ type: 'occupationHeroImage', code: $scope.occupationMajorGroup.onetCode }]
      });

      $scope.uploader.onCompleteItem = function (fileItem) {
        console.info('onCompleteItem ', fileItem);

        // refresh our object since URLs should have changed - and this object doesn't get notifications
        // (don't need to do anything with the result as fresh data will be added to our existing $scope.occupationMajorGroup object)
        siteService.getOccupationAuxiliaryData(occupationMajorGroup.onetCode);
      };
    }
})();
