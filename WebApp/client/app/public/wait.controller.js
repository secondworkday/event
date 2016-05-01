(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('WaitController', WaitController);

    WaitController.$inject = ['$scope', 'siteService'];

    /* @ngInject */
    function WaitController($scope, siteService) {
      
    }
})();
