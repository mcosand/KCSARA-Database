var DatabaseAccountModel = function (dto) {
  this.merge = function (newData) {
    $.extend(this, newData);
    this.lastActive = moment(this.lastActive);
    this.lastPassword = moment(this.lastPassword);
    this.lastLocked = this.lastLocked ? moment(this.lastLocked) : null;
  }
  this.merge(dto);

  this.groups = [];
  this.groups.loaded = false;
  this.linkedMember = {};
  this.linkedMember.loaded = false;
};