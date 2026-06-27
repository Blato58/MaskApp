package cn.com.heaton.shiningmask.model.bean;

/* JADX INFO: loaded from: classes.dex */
public class Language {
    private String abbreviation;
    private String languageName;

    public Language(String str, String str2) {
        this.abbreviation = str;
        this.languageName = str2;
    }

    public String getAbbreviation() {
        return this.abbreviation;
    }

    public void setAbbreviation(String str) {
        this.abbreviation = str;
    }

    public String getLanguageName() {
        return this.languageName;
    }

    public void setLanguageName(String str) {
        this.languageName = str;
    }
}