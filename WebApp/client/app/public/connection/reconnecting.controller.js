(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('ReconnectingController', ReconnectingController);

    ReconnectingController.$inject = ['$scope', 'siteService'];

    /* @ngInject */
    function ReconnectingController($scope, siteService) {
      
    }
})();
