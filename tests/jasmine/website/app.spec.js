describe("app.js", function () {
  describe("UserModel", function () {
    it("should be defined", function () {
      expect(typeof UserModel).toBe("function");
    });

    it("should load data", function () {
      var loaded = (new UserModel()).load({ username: 'test user', isAuthenticated: true });
      expect(loaded.username()).toBe("test user");
      expect(loaded.isAuthenticated()).toBe(true);
    })
  });

  describe("App", function () {
    it("should be defined with properties", function () {
      var model = new App();
      expect(typeof model.page).not.toBe('undefined');
      expect(ko.isObservable(model.user)).toBe(true);
    });
    it("should bootstrap", function () {
      var pageModel = new Object();
      var boot = App.bootstrap(pageModel);
      expect(typeof boot).toBe("function");
      boot();
    })
  })

  describe("getCookie", function () {
    it("should parse sample A", function () {
      var data = '{"username":"","isAuthenticated":false}'
      expect(App.getCookie("authInfo", 'authInfo='+data+'; expires=Thu, 12-Feb-2015 10:35:42 GMT; path=/'))
      .toBe(data)
    })
  })
});