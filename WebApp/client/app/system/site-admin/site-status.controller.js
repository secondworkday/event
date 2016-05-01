(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SiteStatusController', SiteStatusController);

    SiteStatusController.$inject = ['$scope', '$log'];

    /* @ngInject */
    function SiteStatusController($scope, $log) {
        $log.debug("Loading SiteStatusController...");
    }
})();
