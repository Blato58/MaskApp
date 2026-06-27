package cn.com.heaton.shiningmask.base.music;

/* JADX INFO: loaded from: classes.dex */
public class Song {
    private String album;
    private int duration;
    private String fileName;
    private String fileUrl;
    private String singer;
    private String size;
    private int state;
    private String title;
    private String type;
    private String year;

    public Song() {
    }

    public Song(String str, String str2, int i, String str3, String str4, String str5, String str6, String str7, String str8) {
        this.fileName = str;
        this.title = str2;
        this.duration = i;
        this.singer = str3;
        this.album = str4;
        this.year = str5;
        this.type = str6;
        this.size = str7;
        this.fileUrl = str8;
    }

    public String getFileName() {
        return this.fileName;
    }

    public void setFileName(String str) {
        this.fileName = str;
    }

    public String getTitle() {
        return this.title;
    }

    public void setTitle(String str) {
        this.title = str;
    }

    public int getDuration() {
        return this.duration;
    }

    public void setDuration(int i) {
        this.duration = i;
    }

    public String getSinger() {
        return this.singer;
    }

    public void setSinger(String str) {
        this.singer = str;
    }

    public String getAlbum() {
        return this.album;
    }

    public void setAlbum(String str) {
        this.album = str;
    }

    public String getYear() {
        return this.year;
    }

    public void setYear(String str) {
        this.year = str;
    }

    public String getType() {
        return this.type;
    }

    public void setType(String str) {
        this.type = str;
    }

    public String getSize() {
        return this.size;
    }

    public void setSize(String str) {
        this.size = str;
    }

    public String getFileUrl() {
        return this.fileUrl;
    }

    public void setFileUrl(String str) {
        this.fileUrl = str;
    }

    public int getState() {
        return this.state;
    }

    public void setState(int i) {
        this.state = i;
    }

    public String toString() {
        return "Song [fileName=" + this.fileName + ", title=" + this.title + ", duration=" + this.duration + ", singer=" + this.singer + ", album=" + this.album + ", year=" + this.year + ", type=" + this.type + ", size=" + this.size + ", fileUrl=" + this.fileUrl + "]";
    }
}