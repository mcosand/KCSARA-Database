/// <binding BeforeBuild='default' Clean='clean' ProjectOpened='default' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");

var paths = {
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/site.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";
paths.controllers = paths.webroot + "js/controllers/";

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("lib:ui.bootstrap", function () {
  return gulp.src([
    paths.webroot + "lib/ui.bootstrap/src/stackedMap/stackedMap.js",
    paths.webroot + "lib/ui.bootstrap/src/modal/modal.js"
  ])
  .pipe(concat(paths.webroot + "lib/ui.bootstrap/localbuild.min.js"))
  .pipe(uglify())
  .pipe(gulp.dest("."));
})

gulp.task("lib:pack", function () {
  return gulp.src([
    paths.webroot + "lib/jquery/dist/jquery.min.js",
    paths.webroot + "lib/bootstrap/dist/js/bootstrap.js",
    paths.webroot + "lib/angular/angular.min.js",
    paths.webroot + "lib/angular-animate/angular-animate.min.js",
    paths.webroot + "lib/angular-aria/angular-aria.min.js",
    paths.webroot + "lib/angular-material/angular-material.min.js",
    paths.webroot + "lib/np-autocomplete/dist/np-autocomplete.min.js",
    paths.webroot + "lib/ui.bootstrap/localbuild.min.js",
    paths.webroot + "lib/ng-file-upload/ng-file-upload-shim.min.js",
    paths.webroot + "lib/ng-file-upload/ng-file-upload.min.js",
    paths.webroot + "lib/AdminLTE/dist/js/app.min.js",
    paths.webroot + "lib/moment/min/moment.min.js"
  ])
  .pipe(concat(paths.webroot + "lib/packed.js"))
  .pipe(gulp.dest("."));
})

gulp.task("sar-database:js", function () {
  return gulp.src([
    paths.webroot + "js/services/api-loader.js",
    paths.webroot + "js/services/events-service.js",
    paths.webroot + "js/services/members-service.js",
    paths.webroot + "js/services/units-service.js",
    paths.webroot + "js/factories/login-recover.js",
    paths.controllers + "dashboard.js",
    paths.controllers + "login-modal.js",
    paths.controllers + "main-search.js",
    paths.controllers + "events/list.js",
    paths.controllers + "events/edit.js",
    paths.controllers + "events/event-view.js",
    paths.controllers + "events/view-roster.js",
    paths.controllers + "events/view-logs.js",
    paths.controllers + "events/view-docs.js",
    paths.controllers + "events/base-editor.js",
    paths.controllers + "events/create-log",
    paths.controllers + "events/create-document.js",
    paths.controllers + "members/members-dashboard.js",
    paths.controllers + "members/members-detail.js",
    paths.controllers + "members/view-contacts.js",
    paths.controllers + "members/view-events.js",
    paths.controllers + "members/view-info.js",
    paths.controllers + "members/view-training.js",
    paths.controllers + "units/unit-roster.js"
  ])
    .pipe(concat(paths.webroot + "js/sar-database.min.js"))
    .pipe(uglify())
    .pipe(gulp.dest("."));
});

gulp.task("sar-map:js", function () {
  return gulp.src([paths.webroot + "js/lib/mapbox.js/mapbox.js", paths.webroot + "js/lib/leaflet.google.js", paths.webroot + "js/lib/TileLayer.GeoJSON.js", paths.webroot + "js/maps/map.js"])
        .pipe(concat(paths.webroot + "js/sar-maps.min.js"))
        .pipe(uglify())
        .pipe(gulp.dest("."))
})

gulp.task("sar-map:css", function () {
  return gulp.src([paths.webroot + "css/site-mapbox.css", "!" + paths.minCss])
      .pipe(concat(paths.webroot + "css/sar-map.min.css"))
      .pipe(cssmin())
      .pipe(gulp.dest("."));
})

gulp.task("sar-map", ["sar-map:js", "sar-map:css"])

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("lib:js", ["lib:ui.bootstrap", "lib:pack"]);
gulp.task("js", ["lib:js", "sar-database:js", "sar-map:js"]);

//gulp.task("min", ["min:js", "sar-database", "min:css"]);

gulp.task("default", ["js", "sar-map", "min:css"]);
