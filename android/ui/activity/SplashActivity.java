package cn.com.heaton.shiningmask.ui.activity;

import android.os.Handler;
import android.view.LayoutInflater;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.app.UploadManager;
import cn.com.heaton.shiningmask.databinding.ActivitySplashBinding;
import cn.com.heaton.shiningmask.ui.utils.SPUtils;

/* JADX INFO: loaded from: classes.dex */
public class SplashActivity extends BaseActivity<ActivitySplashBinding> {
    private static final int SHOW_TIME_MIN = 1500;

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivitySplashBinding inflateBinding(LayoutInflater layoutInflater) {
        if (!isTaskRoot()) {
            finish();
        }
        return ActivitySplashBinding.inflate(layoutInflater);
    }

    public void uploadInstall() {
        if (((Boolean) SPUtils.get(getApplication(), C.SP.FIRST_INSTALL, true)).booleanValue()) {
            UploadManager.uploadInstallInfo(getApplicationContext());
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.SplashActivity.1
            @Override // java.lang.Runnable
            public void run() {
                SplashActivity.this.toActivity(ConnectActivity.class);
                SplashActivity.this.finish();
            }
        }, 1500L);
        uploadInstall();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
    }
}