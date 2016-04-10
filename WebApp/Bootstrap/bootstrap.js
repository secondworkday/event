
app.controller('BootstrapController', function ($scope, $state, $http, $window, $msUI, bootstrapService) {

  $scope.credentials = {};

  // The server provides a set of Guest users via the ASPX page
  $scope.siteConfigData = $state.current.data;

  $scope.progressMessages = [];

  $scope.s3AccountsDatabaseNames = [];
  $scope.localAccountsDatabaseNames = [];


  $scope.tabs = [
    {
      "title": "Remote Database",
      "fields": ['siteName', 'remoteServerName', 'remoteRuntimeServerUserPasswordMasked', 'accountsDatabaseName']
    },
    {
      "title": "Local Database",
      "fields": ['siteName', 'localServerName', 'localRuntimeServerUserPasswordMasked', 'localAccountsDatabaseName']
    },
    {
      "title": "Create Local Database",
      "fields": ['siteName', 'localServerName', 'localServerSqlAdminName', 'localRuntimeServerUserPasswordMasked', 'accountsDatabaseName', 'createDatabase']
    },
    {
      "title": "Download S3 Database",
      "fields": ['siteName', 'localServerName', 'localServerSqlAdminName', 'localRuntimeServerUserPasswordMasked', 'awsAccessKey', 's3AccountsDatabaseName']
    }
  ];
  $scope.tabs.activeTab = 2;
  $scope.databaseContent = 'create';


  $scope.remoteDatabase = function (siteConfigData) {
    bootstrapService.remoteDatabase(siteConfigData)
        .then(function (successData) {
          $.connection.hub.stop();
          document.location = "/";
        }, function (failureData) {
          // failure
        }, function (progressData) {
          // progress
          $scope.progressMessages.push(progressData);
        });
  };

  $scope.localDatabase = function (siteConfigData) {
    bootstrapService.localDatabase(siteConfigData)
        .then(function (successData) {
          $.connection.hub.stop();
          document.location = "/";
        }, function (failureData) {
          // failure
        }, function (progressData) {
          // progress
          $scope.progressMessages.push(progressData);
        });
  };

  $scope.createLocalDatabase = function (siteConfigData) {
    bootstrapService.createLocalDatabase(siteConfigData)
        .then(function (successData) {
          $.connection.hub.stop();
          document.location = "/";
        }, function (failureData) {
          // failure
        }, function (progressData) {
          // progress
          $scope.progressMessages.push(progressData);
        });
  };

  $scope.downloadS3Database = function (siteConfigData) {
    bootstrapService.downloadS3Database(siteConfigData)
        .then(function (successData) {
          $.connection.hub.stop();
          document.location = "/";
        }, function (failureData) {
          // failure
        }, function (progressData) {
          // progress
          // Since we've sucessfully sent a message, model.myConversation should be valid
          $msUI.showToast(progressData, "DownloadS3Database");
        });
  };


  function getSqlServerLogins(siteConfigData) {
    if ($scope.siteConfigData.localServerName && $scope.siteConfigData.localServerSqlAdminName && $scope.siteConfigData.localServerSqlAdminPasswordMasked) {
      bootstrapService.getSqlServerLogins($scope.siteConfigData.localServerName, $scope.siteConfigData.localServerSqlAdminName, $scope.siteConfigData.localServerSqlAdminPasswordMasked)
          .then(function (successData) {
            console.log("server logins", successData);
            $scope.localAccountsDatabaseNames = successData.accountsDatabaseNames;
          }, function (failureData) {
            // failure
          }, function (progressData) {
          });
    }
  };


  function updateLocalAccountsDatabaseNames(siteConfigData) {
    if ($scope.siteConfigData.localServerName && $scope.siteConfigData.localServerSqlAdminName && $scope.siteConfigData.localServerSqlAdminPasswordMasked) {
      bootstrapService.getSqlServerAccountsDatabaseNames($scope.siteConfigData.localServerName, $scope.siteConfigData.localServerSqlAdminName, $scope.siteConfigData.localServerSqlAdminPasswordMasked)
          .then(function (successData) {
            $scope.localAccountsDatabaseNames = successData.accountsDatabaseNames;
          }, function (failureData) {
            // failure
          }, function (progressData) {
          });
    }
  };

  function updateS3AccountsDatabaseNames(siteConfigData) {
    if ($scope.siteConfigData.awsAccessKey && $scope.siteConfigData.awsSecretAccessKey) {
      bootstrapService.getS3AccountsDatabaseNames($scope.siteConfigData.awsAccessKey, $scope.siteConfigData.awsSecretAccessKey)
          .then(function (successData) {
            $scope.s3AccountsDatabaseNames = successData.accountsDatabaseNames;
          }, function (failureData) {
            // failure
          }, function (progressData) {
          });
    }
  };


  // init

  getSqlServerLogins($scope.siteConfigData);



  updateLocalAccountsDatabaseNames($scope.siteConfigData);
  updateS3AccountsDatabaseNames($scope.siteConfigData);
});