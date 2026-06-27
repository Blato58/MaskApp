package cn.com.heaton.shiningmask.base;

import android.content.Context;
import android.util.Log;
import cn.com.heaton.shiningmask.base.app.crash.CrashHandler;
import cn.com.heaton.shiningmask.base.app.crash.CrashInfo;
import cn.com.heaton.shiningmask.dao.DaoMaster;
import cn.com.heaton.shiningmask.dao.DaoSession;
import cn.com.heaton.shiningmask.model.data.AppData;
import com.cdbwsoft.library.BaseApplication;
import csh.tiro.cc.aes;

/* JADX INFO: loaded from: classes.dex */
public class App extends BaseApplication {
    private static String DATA_BASE_NAME = "folight";
    private static App app;
    private static AppData appData = new AppData();
    private static DaoSession mDaoSession;

    @Override // com.cdbwsoft.library.BaseApplication, android.app.Application
    public void onCreate() {
        super.onCreate();
        app = this;
        aes.keyExpansionDefault();
        initDatabase(this);
        new CrashHandler.Builder().crashUploader(new CrashHandler.CrashUploader() { // from class: cn.com.heaton.shiningmask.base.App.1
            @Override // cn.com.heaton.shiningmask.base.app.crash.CrashHandler.CrashUploader
            public void crashMessage(CrashInfo crashInfo) {
                Log.e("MainApplication", "uploadCrashMessage: ");
            }
        }).build().init(this);
    }

    public static App getInstance() {
        return app;
    }

    private void initDatabase(Context context) {
        mDaoSession = new DaoMaster(new DaoMaster.DevOpenHelper(context, DATA_BASE_NAME).getWritableDb()).newSession();
    }

    public static DaoSession getDaoSession() {
        return mDaoSession;
    }

    public static AppData getAppData() {
        return appData;
    }
}