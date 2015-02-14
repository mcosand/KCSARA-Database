describe("LoginModel.js", function () {
  it("should be defined", function () {
    expect(typeof LoginModel).toBe("function");
  });
  it("can be instantiated", function () {
    expect(new LoginModel()).not.toBe(null);
  })
});
