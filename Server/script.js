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

  const response = await axios.get(apiAvailability);
  const xmlData = response.data;
  getBookInformation(xmlData, id, (newBook) => {
    console.log(newBook);

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

    const response = await axios.get(apiAvailability);
    const xmlData = response.data;
    getBookInformation(xmlData, id, async (newBook) => {
      console.log(newBook);

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
  //get ids from books and add them to idArray
  try {
    // get data from api

    for (i = 97; i <= 122; i++) {
      const response = await axios.get(
        `https://cataloguswebservices.bibliotheek.be/staging/geel/search/?q=
        ${String.fromCharCode(i)}
        &authorization=629e87cf5fe7767339231c6e0e1307ec&refine=true&s=cover`
      );

      const xmlData = response.data;
      xmlParser.parseString(xmlData, (err, data) => {
        //parse date form xml to json
        if (err) throw err;
        const jsonData = JSON.stringify(data.aquabrowser.results.result); // get required property
        const jsonObj = JSON.parse(jsonData);

        allRandomBooks.bookIds.push(jsonObj.map((obj) => obj.id._));
        allRandomBooks.bookCovers.push(
          jsonObj.map((obj) => obj.coverimages.coverimage._)
        );
      });
    }

    allRandomBooks.bookIds = allRandomBooks.bookIds.flat(1);
    allRandomBooks.bookCovers = allRandomBooks.bookCovers.flat(1);
    console.log("Done Extracting IDs...");
    await extractIsbn();
  } catch (error) {
    console.error(error);
  }
}

async function extractIsbn() {
  console.log("Extracting ISBNs...");

  // get isbn from book using cover link, also possible using another endpoint if preffered
  try {
    Array.from(allRandomBooks.bookCovers).forEach((element) => {
      //extract ISBN from bookcover link
      const text = element;
      const searchString = "ISBN=";

      const index = text.indexOf(searchString);
      if (index >= 0) {
        const selectedText = text
          .substring(index + searchString.length)
          .substring(0, 13); // select only ISBN from bookcover link
        let temp = allRandomBooks.IsbnNummers;
        temp.push(selectedText);
        allRandomBooks.IsbnNummers = temp;
      } else {
        console.log(`"${searchString}" not found in "${text}"`);
      }
    });
    console.log("Done Extracting ISBNs...");
    await removeUnavailable();
  } catch (error) {
    console.error(error);
  }
}

async function removeUnavailable() {
  console.log("Removing Unavailables...");

  // Remove unavailable books from all arrays
  try {
    allRandomBooks.bookIds.forEach(async (bookIdElem) => {
      const apiAvailability =
        "https://cataloguswebservices.bibliotheek.be/staging/geel/availability/?id=" +
        bookIdElem +
        "&authorization=629e87cf5fe7767339231c6e0e1307ec"; // availabilty api using id
      const response = await axios.get(apiAvailability);
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
    });
    console.log("Done Removing Unavailables...");
    const PORT = process.env.PORT || 3000;
    app.listen(PORT, () => {
      console.log(`Server listening on port ${PORT}`);
    });
  } catch (error) {
    console.error(error);
  }
}
