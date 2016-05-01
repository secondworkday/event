(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SiteAdminDashboardController', SiteAdminDashboardController);

    SiteAdminDashboardController.$inject = ['$scope', '$log'];

    /* @ngInject */
    function SiteAdminDashboardController($scope, $log) {
        $log.debug("Loading SiteAdminDashboardController...");
    }
})();
