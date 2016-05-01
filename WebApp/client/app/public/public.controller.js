(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('PublicController', PublicController);

    PublicController.$inject = ['$scope', '$log', 'siteService'];

    /* @ngInject */
    function PublicController($scope, $log, siteService) {
      $log.debug('Loading PublicController...');
    }
})();
