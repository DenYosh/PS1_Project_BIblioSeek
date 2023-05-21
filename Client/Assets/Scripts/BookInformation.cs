using UnityEngine;


public class BookInformation
{
    public string bookTitle;
    public string bookWriter;
    public string bookZone;
    public string bookZoneExtra;
    public string bookZoneImage;
}

public class BookInformationImage
{
    public BookInformationImage(string bookTitle, string bookWriter, string bookZone, string bookZoneExtra, Sprite bookZoneImage, Sprite bookCover)
    {
        this.bookTitle = bookTitle;
        this.bookWriter = bookWriter;
        this.bookZone = bookZone;
        this.bookZoneExtra = bookZoneExtra;
        this.bookZoneImage = bookZoneImage;
        this.bookCover = bookCover;
    }

    public string bookTitle;
    public string bookWriter;
    public string bookZone;
    public string bookZoneExtra;
    public Sprite bookZoneImage;  

    public Sprite bookCover;
}


public class CoverInformation
{
    public string Isbn;
    public string bookCover;
    public string bookId;
}

public class CoverInformationImage
{
    public CoverInformationImage(string isbnNumber, Texture2D bookCoverImage, string bookId)
    {
        IsbnNumber = isbnNumber;
        this.Image = bookCoverImage;
        this.bookId = bookId;
    }

    public string bookId;
    public string IsbnNumber;
    public Texture2D Image;
}