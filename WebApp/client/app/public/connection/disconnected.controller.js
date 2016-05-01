(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('DisconnectedController', DisconnectedController);

    DisconnectedController.$inject = ['$scope', 'utilityService', 'siteService'];

    /* @ngInject */
    function DisconnectedController($scope, utilityService, siteService) {
      $scope.reloadPage = function () {
        utilityService.restartConnection();
      };

    }
})();
