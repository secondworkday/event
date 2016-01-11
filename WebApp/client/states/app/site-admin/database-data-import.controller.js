app.controller('DatabaseDataImportController', function ($scope, $log, FileUploader, utilityService, siteService) {
  $log.debug('Loading DatabaseDataImportController...');

  $scope.reviewReady = false;
  $scope.processingFile = false;

  //!! TODO pull in the states
  $scope.sampleDataImportStates = [
    {
      name: "Georgia",
      lastUpdateDate: "2015-10-01T03:24:00",
      programCount: 99,
      providerCount: 99,
      useEnhancedData: false
    },
    {
      name: "California",
      lastUpdateDate: "2015-08-01T03:24:00",
      programCount: 999,
      providerCount: 999,
      useEnhancedData: true
    }
  ];

  $scope.subTypes = [
    { name: "Education Providers", code: {subType: 'providers'} },
    { name: "Education Programs", code: {subType: 'programs'} }
  ];

  $scope.sampleData = {
    tableHeaders: ["ID", "Name", "Number"],
    tableRows: [
      [101, "Burger King", 981422],
      [102, "McDonalds", 348901],
      [103, "Chipotle", 438791]
    ]
  };

  $scope.sampleFiles = [
    {
      fileName: "TORQ_129832 V3.xlsx",
      uploadDate: "2015-12-11T03:24:00",
      size: 5892
    },
    {
      fileName: "LI_34981_Data Education Prov - Julia.xlsx",
      uploadDate: "2015-10-01T03:24:00",
      size: 18925
    }
  ];

  $scope.sampleWarnings = [
    "Uploaded file contained formatting that cannot be processed by the server. Use unformatted .CSV files if possible."
  ];

  $scope.sampleErrors = [
    "Too many columns. Expected 4 columns, uploaded file contained 6 columns.",
    "Incorrect data type.  Expected number, uploaded file contained a string."
  ];

  $scope.startOver = function () {
    $scope.reviewReady = false;
    $scope.processingFile = false;
  };

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
    formData: [{ type: 'wioaEducationData' }]
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

    siteService.getEducationInfo()
    .then(function (successData) {
      $scope.files = successData.files;
    });



    // refresh our object since URLs should have changed - and this object doesn't get notifications
    // (don't need to do anything with the result as fresh data will be added to our existing $scope.occupationMajorGroup object)
    //!! siteService.getOccupationAuxiliaryData(occupationMajorGroup.onetCode);
  };

  siteService.getEducationInfo()
  .then(function (successData) {
    $scope.files = successData.files;
  });


});
