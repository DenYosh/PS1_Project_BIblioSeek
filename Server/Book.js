class Book {
  title = "";
  writer = "";
  zone = "";
  extra = "";
  image = null;

  constructor(title, writer, zone, extra, image) {
    this.title = title;
    this.writer = writer;
    this.zone = zone;
    this.extra = extra;
    if (image.genre) {
      this.image = image.genre.$.src;
    } else if (image.zizo) {
      this.image = image.zizo.$.image;
    }
  }
}

module.exports = Book;
