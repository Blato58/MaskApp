package cn.com.heaton.shiningmask.model;

import cn.com.heaton.shiningmask.model.db.AppDbHelper;
import cn.com.heaton.shiningmask.model.http.AppApiHelper;
import cn.com.heaton.shiningmask.model.preference.AppPreferenceHelper;

/* JADX INFO: loaded from: classes.dex */
public class DataManager implements AppDbHelper, AppApiHelper, AppPreferenceHelper {
    private AppApiHelper mAppApiHelper;
    private AppDbHelper mAppDbHelper;
    private AppPreferenceHelper mAppPreferenceHelper;

    public DataManager(AppDbHelper appDbHelper, AppApiHelper appApiHelper, AppPreferenceHelper appPreferenceHelper) {
        this.mAppDbHelper = appDbHelper;
        this.mAppApiHelper = appApiHelper;
        this.mAppPreferenceHelper = appPreferenceHelper;
    }

    @Override // cn.com.heaton.shiningmask.model.db.AppDbHelper
    public void testDb() {
        this.mAppDbHelper.testDb();
    }

    @Override // cn.com.heaton.shiningmask.model.http.AppApiHelper
    public void testRequestNetwork() {
        this.mAppApiHelper.testRequestNetwork();
    }

    @Override // cn.com.heaton.shiningmask.model.preference.AppPreferenceHelper
    public void testPreference() {
        this.mAppPreferenceHelper.testPreference();
    }
}