(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('ReportTemplatesController', ReportTemplatesController);

    ReportTemplatesController.$inject = ['$scope', '$log', 'FileUploader', 'utilityService', 'siteService'];

    /* @ngInject */
    function ReportTemplatesController($scope, $log, FileUploader, utilityService, siteService) {
      $log.debug('Loading ReportTemplatesController...');

      $scope.uploadFile = function() {
        $scope.processingFile = true;
        // process the file...then when it's ready set processingFile to false and reviewReady to true
        $scope.processingFile = false;
        $scope.reviewReady = true;
      };

      $scope.uploader = new FileUploader({
        url: '/upload.ashx',
        autoUpload: true,
        removeAfterUpload: true,
        formData: [{ type: 'reportTemplate' }]
      });

      $scope.setFileUploaderItemData = function (itemData) {
        //!! I'm not understanding something here - this function is called twice, with the second time homeID always being 1. So only remember the first one
        if (!$scope.uploader.itemData) {
          $scope.uploader.itemData = itemData;
        }
      };

      $scope.uploader.onAfterAddingFile = function (fileItem) {
        console.info('onAfterAddingFile', fileItem);
        fileItem.formData.push($scope.uploader.itemData);
        //!! I'm not understanding something here - clear our homeID value so we remember the next one correctly
        $scope.uploader.itemData = undefined;
      };



      $scope.uploader.onCompleteItem = function (fileItem, response, status, headers) {

        console.info('onCompleteItem ', fileItem);
        console.info('onCompleteItem ', response);
        console.info('onCompleteItem ', status);
        console.info('onCompleteItem ', headers);

        $scope.reviewReady = false;
        $scope.warnings = undefined;
        $scope.errors = undefined;
        $scope.tableData = undefined;

        if (response.StatusCode == 400) {
          var responseData = response.ResponseData;
          $scope.warnings = responseData.warnings;
          $scope.errors = responseData.errors;
          $scope.tableData = responseData.tableData;
          $scope.reviewReady = true;
        }

        $scope.processingFile = false;

        utilityService.getReportTemplateInfo()
        .then(function (successData) {
          $scope.reportTemplates = successData;
        });



        // refresh our object since URLs should have changed - and this object doesn't get notifications
        // (don't need to do anything with the result as fresh data will be added to our existing $scope.occupationMajorGroup object)
        //!! siteService.getOccupationAuxiliaryData(occupationMajorGroup.onetCode);
      };

      utilityService.getReportTemplateInfo()
      .then(function (successData) {
        $scope.reportTemplates = successData;
      });

      $scope.download = function(){
        // should download requested file
      };

      $scope.remove = function(template){
      };



      $scope.removeReportTemplateOverrideFile = function (reportTemplate) {

        utilityService.removeReportTemplateOverrideFile(reportTemplate.fileName)
        .then(function (successData) {
          $scope.reportTemplates = successData;
        });

      };
    }
})();
