app.controller('OpsBackupController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading OpsBackupController...');

  $scope.sqlAccountsDatabaseNames = [];
  $scope.sqlReferenceDatabaseNames = [];
  $scope.s3AccountsDatabaseNames = [];
  $scope.s3ReferenceDatabaseNames = [];

  function getSqlAccountsDatabaseNames() {
      utilityService.getSqlAccountsDatabaseNames()
          .then(function (successData) {
              $scope.sqlAccountsDatabaseNames = successData.databaseNames;
          });
  };

  function getSqlReferenceDatabaseNames() {
      utilityService.getSqlReferenceDatabaseNames()
          .then(function (successData) {
              $scope.sqlReferenceDatabaseNames = successData.databaseNames;
          });
  };

  function getS3AccountsDatabaseNames() {
      utilityService.getS3AccountsDatabaseNames()
          .then(function (successData) {
              $scope.s3AccountsDatabaseNames = successData.databaseNames;
          });
  };

  function getS3ReferenceDatabaseNames() {
      utilityService.getS3ReferenceDatabaseNames()
          .then(function (successData) {
              $scope.s3ReferenceDatabaseNames = successData.databaseNames;
          });
  };


  $scope.backupAccountsDatabase = function (databaseName) {
      return utilityService.backupAccountsDatabase(databaseName);
  }
  $scope.backupReferenceDatabase = function (databaseName) {
      return utilityService.backupReferenceDatabase(databaseName);
  }
  $scope.restoreAccountsDatabase = function (databaseName) {
      return utilityService.restoreAccountsDatabase(databaseName);
  }
  $scope.restoreReferenceDatabase = function (databaseName) {
      return utilityService.restoreReferenceDatabase(databaseName);
  }


  // init
  getSqlAccountsDatabaseNames();
  getSqlReferenceDatabaseNames();
  getS3AccountsDatabaseNames();
  getS3ReferenceDatabaseNames();

});
