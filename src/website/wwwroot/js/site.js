angular.module('sar-database', ['ngMaterial', 'ui.router.title', 'ui.router', 'ngResource', 'md.data.table', 'restangular'])


.config(['$mdThemingProvider', function ($mdThemingProvider) {
  //http://mcg.mbitson.com/
  $mdThemingProvider.definePalette('sar-yellow', {
    '50': '#ffffff',
    '100': '#fce7c6',
    '200': '#f9d190',
    '300': '#f6b44c',
    '400': '#f4a82f',
    '500': '#f39c12',
    '600': '#db8b0b',
    '700': '#be780a',
    '800': '#a16608',
    '900': '#845307',
    'A100': '#ffffff',
    'A200': '#fce7c6',
    'A400': '#f4a82f',
    'A700': '#be780a',
    'contrastDefaultColor': 'light',
    'contrastDarkColors': '50 100 200 300 400 A100 A200 A400 A700'
  })
  .definePalette('sar-green', {
    '50': '#97ffb1',
    '100': '#4bff77',
    '200': '#13ff4d',
    '300': '#00ca32',
    '400': '#00ac2b',
    '500': '#008d23',
    '600': '#006e1b',
    '700': '#005014',
    '800': '#00310c',
    '900': '#001305',
    'A100': '#97ffb1', // hue-1
    'A200': '#008d23', // 
    'A400': '#00ac2b', // hue-2
    'A700': '#005014', // hue-3
    'contrastDefaultColor': 'light',
    'contrastDarkColors': '50 100 200 300 A100 A200'
  });;

  $mdThemingProvider.theme('default')
  .primaryPalette('sar-yellow')
  .accentPalette('sar-green')
  .backgroundPalette('grey');
}])

  .filter('nospace', function () {
    return function (value) {
      return (!value) ? '' : value.replace(/ /g, '');
    };
  })
.filter('humanizeDoc', function () {
  return function (doc) {
    if (!doc) return;
    if (doc.type === 'directive') {
      return doc.name.replace(/([A-Z])/g, function ($1) {
        return '-' + $1.toLowerCase();
      });
    }
    return doc.label || doc.name;
  };
})

.factory('httpRequestInterceptor', ['currentUser', function (currentUser) {
  return {
    request: function (config) {
      if (config.url.indexOf('/api') == 0 && currentUser.user && currentUser.user.access_token) {
        config.headers['Authorization'] = 'Bearer ' + currentUser.user.access_token;
      }
      return config;
    }
  };
}])

.config(function ($httpProvider) {
  $httpProvider.interceptors.push('httpRequestInterceptor');
})


.controller('FrameCtrl', ['authService', '$scope', '$mdSidenav', '$http', '$location', '$window', '$timeout', '$rootScope',
  function (authService, $scope, $mdSidenav, $http, $location, $window, $timeout, $rootScope) {
  var self = this;

  function readUser() {
    var user = authService.getUser().then(function (user) {
      $scope.user = user
        ? angular.extend({ isLoggedIn: true }, user.profile)
        : { isLoggedIn: false };
    });
  }

  authService.subscribe($scope, readUser);

  this.isSelected = function isSelected(page) {
    return menu.isPageSelected(page);
  };

  this.isSectionSelected = function isSectionSelected(section) {
    var selected = false;
    var openedSection = $scope.menu.openedSection;
    if (openedSection === section) {
      selected = true;
    }
    else if (section.children) {
      section.children.forEach(function (childSection) {
        if (childSection === openedSection) {
          selected = true;
        }
      });
    }
    return selected;
  };

  this.isOpen = function isOpen(section) {
    return menu.isSectionSelected(section);
  };

  this.toggleOpen = function toggleOpen(section) {
    menu.toggleSelectSection(section);
  };

  $rootScope.$on('$locationChangeSuccess', onLocationChange);
  var menu;
  function onLocationChange() {
    var returnUrl = $window.sessionStorage.getItem('oidc:returnUrl');
    if (returnUrl) {
      debugger;
      $window.sessionStorage.removeItem('oidc:returnUrl')
      if (returnUrl != $location.url()) $window.location.href = returnUrl;
    }

    $scope.closeMainMenu();

    var path = $location.path();

    if (path == '/') {
      menu.selectSection(null);
      menu.selectPage(null, null);
      return;
    }

    var matchPage = function (section, page) {
      if (path.indexOf(page.url) !== -1) {
        menu.selectSection(section);
        menu.selectPage(section, page);
      }
    };

    menu.sections.forEach(function (section) {
      if (section.children) {
        // matches nested section toggles, such as API or Customization
        section.children.forEach(function (childSection) {
          if (childSection.pages) {
            childSection.pages.forEach(function (page) {
              matchPage(childSection, page);
            });
          }
        });
      }
      else if (section.pages) {
        // matches top-level section toggles, such as Demos
        section.pages.forEach(function (page) {
          matchPage(section, page);
        });
      }
      else if (section.type === 'link') {
        // matches top-level links, such as "Getting Started"
        matchPage(section, section);
      }
    });
  }

  menu = {
    sections: [
      { name: 'Main Menu', type: 'heading' },
      { name: "Members", type: 'link', icon: 'person', url: window.appRoot + 'members' },
      {
        name: "Missions", type: 'toggle', icon: 'terrain', pages:
        [
          { name: "Rosters", type: 'link', url: window.appRoot + "missions/list" },
          { name: "Reports", type: "link", url: window.appRoot + "missions/yearly" }
        ]
      },
      {
        name: 'Training', type: 'toggle', icon: 'school', pages:
          [
            { name: "Rosters", type: "link", url: window.appRoot + "training/list" },
            { name: "Reports", type: "link", url: window.appRoot + "training" }
          ]
      },
      { name: "Units", type: 'link', icon: 'group', url: window.appRoot + 'units' },
      { name: "Animals", type: 'link', icon: 'pets', url: window.appRoot + 'animals' },
      { name: 'Admin Menu', type: 'heading' },
      { name: 'My Account', type: 'link', url: window.appRoot + "accounts/me" }
    ],
    selectSection: function (section) {
      menu.openedSection = section;
    },
    toggleSelectSection: function (section) {
      menu.openedSection = (menu.openedSection === section ? null : section);
    },
    isSectionSelected: function (section) {
      return menu.openedSection === section;
    },

    selectPage: function (section, page) {
      menu.currentSection = section;
      menu.currentPage = page;
    },
    isPageSelected: function (page) {
      return menu.currentPage === page;
    }
  };

  angular.extend($scope, {
    mainNavOpen: false,
    showMainMenu: function () { $mdSidenav('mainMenu').open(); $scope.mainNavOpen = true; },
    closeMainMenu: function () { $timeout(function () { $mdSidenav('mainMenu').close(); $scope.mainNavOpen = false; }); },
    menu: menu,
    user: {
      isLoggedIn: false
    },
    auth: {
      menuOpen: false,
      showMenu: function () { $mdSidenav('authMenu').open(); }
    },
    search: {
      menuSearchOpen: false,
      showSearchMenu: function () { $mdSidenav('searchMenu').open(); },
      querySearch: function (text) {
        if (!text) return [];

        return $http({
          method: 'GET',
          url: window.appRoot + 'api/search?q=' + encodeURIComponent(text)
        }).then(function successCallback(response) {
          return response.data;
        }, function errorCallback(response) {
          // called asynchronously if an error occurs
          // or server returns response with an error status.
        });
      },
      selectedItemChange: function (item) {
        if (item.type == 'Member') {
          window.location.href = window.appRoot + 'Members/Detail/' + item.summary.id;
        }
        else if (item.type == "Mission") {
          window.location.href = window.appRoot + 'Missions/Roster/' + item.summary.id;
        }
        else {
          console.log(item);
        }
      }
    },
    doSignout: function () {
      authService.signout();
    },
    doSignin: function () {
      $window.sessionStorage.setItem('oidc:returnUrl', $location.url());
      authService.signin();
    }
  });
  readUser();
}])
;

//Oidc.Log.logger = console;
//Oidc.Log.level = Oidc.Log.INFO;
//var mgr = new Oidc.UserManager(oidcSettings);
//mgr.getUser().then(function (user) {
//});