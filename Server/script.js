const express = require("express");
const axios = require("axios");
const xml2js = require("xml2js");
const url = require("url");
const app = express();
const Book = require("./Book");
const AllRandomBooks = require("./allRandomBooks");
const sharp = require("sharp");

const xmlParser = new xml2js.Parser({ explicitArray: false });

const allRandomBooks = new AllRandomBooks();

extractIds();

app.get("/", (req, res) => {
  res.send("Hello World!");
});

app.get("/cover", async (req, res) => {
  let bookIndex = 0;
  bookIndex = Math.floor(Math.random() * allRandomBooks.IsbnNummers.length);

  res.json({
    Isbn: allRandomBooks.IsbnNummers[bookIndex],
    bookCover:
      allRandomBooks.bookCovers[bookIndex].substring(
        0,
        allRandomBooks.bookCovers[bookIndex].length - 5
      ) + "medium", //Remove small, changed to medium
    bookId: allRandomBooks.bookIds[bookIndex],
  });
});

app.get("/getInfo", async (req, res) => {
  const queryObject = url.parse(req.url, true).query; //get id from url query
  const id = queryObject.Id;
  if (id == null) {
    return;
  }
  const apiAvailability =
    "https://cataloguswebservices.bibliotheek.be/staging/geel/availability/?id=" +
    id +
    "&authorization=629e87cf5fe7767339231c6e0e1307ec"; // availabilty api using id

  const response = await axios.get(apiAvailability).catch((err) => {
    if (err.response) {
      console.log(err.response.status);
    }
  });

  const xmlData = response.data;
  getBookInformation(xmlData, id, (newBook) => {

    res.json({
      bookTitle: newBook.title,
      bookWriter: newBook.writer,
      bookZone: newBook.zone,
      bookZoneExtra: newBook.extra,
      bookZoneImage: `image?Id=${id}`,
    });
  });
});

app.get("/image", async (req, res) => {
  //convert image from gif to jpg and post to website
  try {
    const queryObject = url.parse(req.url, true).query; //get id from url query
    if (queryObject == null) {
      return;
    }
    const id = queryObject.Id;

    const apiAvailability =
      "https://cataloguswebservices.bibliotheek.be/staging/geel/availability/?id=" +
      id +
      "&authorization=629e87cf5fe7767339231c6e0e1307ec"; // availabilty api using id

    const response = await axios.get(apiAvailability).catch((err) => {
      if (err.response) {
        console.log(err.response.status);
      }
    });

    const xmlData = response.data;
    getBookInformation(xmlData, id, async (newBook) => {

      // Fetch the image from the remote URL
      if (newBook.image) {
        const response = await axios.get(newBook.image, {
          responseType: "arraybuffer",
        });

        // Convert the image to JPEG using sharp
        const imageData = await sharp(response.data).jpeg().toBuffer();

        // Send the converted image back to the client
        res.set("Content-Type", "image/jpeg");
        res.send(imageData);
      } else {
        // Convert the image to JPEG using sharp
        const imageData = await sharp("./default.png").jpeg().toBuffer();

        // Send the converted image back to the client
        res.set("Content-Type", "image/jpeg");
        res.send(imageData);
      }
    });
  } catch (error) {
    console.error(error);
    res.sendStatus(500);
  }
});

function getBookInformation(xmlData, id, callback) {
  xmlParser.parseString(xmlData, (err, data) => {
    // parse result xml to json
    if (err) throw err;
    let extraInformationBook = JSON.parse(
      JSON.stringify(data.aquabrowser.locations.location.location.items.item)
    ); //get image, zone, extra
    let mainInformationBook = JSON.parse(
      JSON.stringify(data.aquabrowser.meta.records.record)
    ); //get titel, author
    if (mainInformationBook.title == null) {
      if (mainInformationBook instanceof Array) {
        mainInformationBook.forEach((elem) => {
          if (id.includes(elem.$.nativeid)) {
            mainInformationBook = elem;
            return;
          }
        });
      } else {
        console.log("ERROR main: " + mainInformationBook);
      }
    }

    if (extraInformationBook.subloc == null) {
      if (extraInformationBook instanceof Array) {
        extraInformationBook.forEach((elem) => {
          if (elem.subloc != null) {
            if (elem.subloc.includes("zone")) {
              extraInformationBook = elem;
              return;
            }
            lastElem = elem;
          }
        });
        if (extraInformationBook.subloc == null) {
          extraInformationBook = lastElem;
        }
      } else {
        extraInformationBook.subloc = "";
        console.log("ERROR extra: " + extraInformationBook);
      }
    }

    const newBook = new Book(
      mainInformationBook.title,
      mainInformationBook.author,
      extraInformationBook.subloc.substring(
        0,
        extraInformationBook.subloc.indexOf(":")
      ),
      extraInformationBook.subloc.substring(
        extraInformationBook.subloc.indexOf(":"),
        extraInformationBook.subloc.length
      ),
      extraInformationBook
    );

    callback(newBook);
  });
}

async function extractIds() {
  console.log("Extracting IDs...");
  try {
    const allBooksEndpoints = ["https://cataloguswebservices.bibliotheek.be/geel/search/?q=harry+potter&authorization=629e87cf5fe7767339231c6e0e1307ec&refine=true&s=cover"]
    // Enable the forloop for getting 520 books : currently not optimized for the API (cataloguswebservices.bibliotheek.be)
    // const allBooksEndpoints = [];
    // for (i = 97; i <= 122; i++) {
    //   allBooksEndpoints.push(`https://cataloguswebservices.bibliotheek.be/geel/search/?q=
    //   ${String.fromCharCode(i)}
    //   &authorization=629e87cf5fe7767339231c6e0e1307ec&refine=true&s=cover`);
    // }
    await Promise.all(
      allBooksEndpoints.map(async (url) => {
        const response = await axios.get(url).catch((err) => {
          if (err.response) {
            console.log(err.response.status);
          }
        });
        const xmlData = response.data;
        xmlParser.parseString(xmlData, (err, data) => {
          //parse date form xml to json
          if (err) throw err;
          const jsonData = JSON.stringify(data.aquabrowser.results.result); // get required property
          const jsonObj = JSON.parse(jsonData);

          allRandomBooks.bookIds = allRandomBooks.bookIds.concat(
            jsonObj.map((obj) => obj.id._)
          );
          allRandomBooks.bookCovers = allRandomBooks.bookCovers.concat(
            jsonObj.map((obj) => obj.coverimages.coverimage._)
          );
          allRandomBooks.IsbnNummers = allRandomBooks.IsbnNummers.concat(
            jsonObj.map((obj) => {
              let isbn = null;
              if (Array.isArray(obj.identifiers["isbn-id"])) {
                for (const isbnObj of obj.identifiers["isbn-id"]) {
                  if (isbnObj._) {
                    isbn = isbnObj._;
                    break;
                  }
                }
              } else if (
                obj.identifiers["isbn-id"] &&
                obj.identifiers["isbn-id"]._
              ) {
                isbn = obj.identifiers["isbn-id"]._;
              }

              const ean =
                obj.identifiers["ean-id"] && obj.identifiers["ean-id"]._;

              if (isbn) {
                return isbn;
              } else if (ean) {
                return ean;
              }
              return obj.identifiers["ean-id"][0]._;
            })
          );
        });
      })).then(async () => {
        console.log("Done Extracting IDs...");
        await removeUnavailable();
      });

  } catch (error) {
    console.error(error);
  }
}

async function removeUnavailable() {
  console.log("Removing Unavailables...");

  // Remove unavailable books from all arrays
  try {
    await Promise.all(
      allRandomBooks.bookIds.map(async (bookIdElem) => {
        const apiAvailability =
          "https://cataloguswebservices.bibliotheek.be/geel/availability/?id=" +
          bookIdElem +
          "&authorization=629e87cf5fe7767339231c6e0e1307ec"; // availabilty api using id
        const response = await axios.get(apiAvailability).catch((err) => {
          if (err.response) {
            console.log(err.response.status);
          }
        });

        const xmlData = response.data;
        xmlParser.parseString(xmlData, (err, data) => {
          //parse date form xml to json
          if (err) throw err;
          const jsonData = JSON.stringify(
            data.aquabrowser.locations.location.location.items.item
          ); // get required property
          const jsonObj = JSON.parse(jsonData);

          if (Array.isArray(jsonObj)) {
            //if result is array (multiple of the same book found)
            let aanwezig = false;
            jsonObj.forEach((element) => {
              if (
                element.status.includes("Aanwezig") |
                element.status.includes("aanwezig")
              ) {
                aanwezig = true;
              }
            });
            if (aanwezig) {
              return;
            }
          } else if (jsonObj.status === "Aanwezig") {
            return;
          }
          console.log("Afwezig:", bookIdElem);
          const index = allRandomBooks.bookIds.indexOf(bookIdElem);
          const bookIds = allRandomBooks.bookIds;
          bookIds.splice(index, 1);
          allRandomBooks.bookIds = bookIds;
          const IsbnNummer = allRandomBooks.IsbnNummers;
          IsbnNummer.splice(index, 1);
          allRandomBooks.IsbnNummers = IsbnNummer;
          const bookCovers = allRandomBooks.bookCovers;
          bookCovers.splice(index, 1);
          allRandomBooks.bookCovers = bookCovers;
        });
      })
    ).then(() => {
      console.log("Done Removing Unavailables...");
      const PORT = process.env.PORT || 3000;
      app.listen(PORT, () => {
        console.log(`Server listening on port ${PORT}`);
      });
    });
  } catch (error) {
    console.error(error);
  }
}
