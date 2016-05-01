(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SiteAdminController', SiteAdminController);

    SiteAdminController.$inject = ['$scope', '$log'];

    /* @ngInject */
    function SiteAdminController($scope, $log) {
      $log.debug('Loading SiteAdminController...');

    }
})();
