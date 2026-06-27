package cn.com.heaton.shiningmask.base;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.util.SparseArray;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import androidx.appcompat.app.AppCompatActivity;
import androidx.viewbinding.ViewBinding;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.app.LanguageUtil;

/* JADX INFO: loaded from: classes.dex */
public abstract class BaseActivity<T extends ViewBinding> extends AppCompatActivity {
    private T binding;
    public Activity mActivity;
    public Context mContext;
    private int mPermissionIdx = 16;
    private SparseArray<GrantedResult> mPermissions = new SparseArray<>();
    private ProgressDialog progressDialog;

    protected abstract void bindListener();

    protected abstract T inflateBinding(LayoutInflater layoutInflater);

    protected abstract void initData();

    protected abstract void initView();

    protected boolean isPortrait() {
        return true;
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, androidx.core.app.ComponentActivity, android.app.Activity
    public void onCreate(Bundle bundle) {
        if (bundle != null) {
            bundle.remove("android:support:fragments");
        }
        super.onCreate(bundle);
        this.mActivity = this;
        this.mContext = this;
        setTranslucentStatus();
        T t = (T) inflateBinding(getLayoutInflater());
        this.binding = t;
        setContentView(t.getRoot());
        initView();
        initData();
        bindListener();
        AppManager.getAppManager().addActivity(this);
    }

    protected T getBinding() {
        return this.binding;
    }

    protected void toActivity(Class cls) {
        startActivity(new Intent(this, (Class<?>) cls));
    }

    protected void toActivity(Class cls, Bundle bundle) {
        Intent intent = new Intent(this, (Class<?>) cls);
        intent.putExtra(cls.getSimpleName(), bundle);
        startActivity(intent);
    }

    protected View inflate(int i) {
        return LayoutInflater.from(this).inflate(i, (ViewGroup) null);
    }

    public void setTranslucentStatus() {
        Window window = getWindow();
        window.clearFlags(67108864);
        window.getDecorView().setSystemUiVisibility(1280);
        window.addFlags(Integer.MIN_VALUE);
        setRequestedOrientation(isPortrait() ? 1 : 0);
    }

    public void requestPermission(final String[] strArr, String str, GrantedResult grantedResult) {
        if (grantedResult == null) {
            return;
        }
        grantedResult.mGranted = false;
        if (strArr == null || strArr.length == 0) {
            grantedResult.mGranted = true;
            runOnUiThread(grantedResult);
            return;
        }
        final int i = this.mPermissionIdx;
        this.mPermissionIdx = i + 1;
        this.mPermissions.put(i, grantedResult);
        boolean z = true;
        for (String str2 : strArr) {
            z = z && checkSelfPermission(str2) == 0;
        }
        if (z) {
            grantedResult.mGranted = true;
            runOnUiThread(grantedResult);
            return;
        }
        boolean z2 = true;
        for (String str3 : strArr) {
            z2 = z2 && !shouldShowRequestPermissionRationale(str3);
        }
        if (!z2) {
            new AlertDialog.Builder(this).setMessage(str).setPositiveButton(R.string.btn_ok, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.BaseActivity.2
                @Override // android.content.DialogInterface.OnClickListener
                public void onClick(DialogInterface dialogInterface, int i2) {
                    BaseActivity.this.requestPermissions(strArr, i);
                }
            }).setNegativeButton(R.string.btn_cancel, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.BaseActivity.1
                @Override // android.content.DialogInterface.OnClickListener
                public void onClick(DialogInterface dialogInterface, int i2) {
                    dialogInterface.dismiss();
                    GrantedResult grantedResult2 = (GrantedResult) BaseActivity.this.mPermissions.get(i);
                    if (grantedResult2 == null) {
                        return;
                    }
                    grantedResult2.mGranted = false;
                    BaseActivity.this.runOnUiThread(grantedResult2);
                }
            }).create().show();
        } else {
            requestPermissions(strArr, i);
        }
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    public void onRequestPermissionsResult(int i, String[] strArr, int[] iArr) {
        super.onRequestPermissionsResult(i, strArr, iArr);
        GrantedResult grantedResult = this.mPermissions.get(i);
        if (grantedResult == null) {
            return;
        }
        if (iArr.length > 0 && iArr[0] == 0) {
            grantedResult.mGranted = true;
        }
        runOnUiThread(grantedResult);
    }

    @Override // androidx.appcompat.app.AppCompatActivity, android.app.Activity, android.view.ContextThemeWrapper, android.content.ContextWrapper
    protected void attachBaseContext(Context context) {
        super.attachBaseContext(LanguageUtil.attachBaseContext(context, LanguageUtil.getSaveLanguage(context)));
    }

    public static abstract class GrantedResult implements Runnable {
        private boolean mGranted;

        public abstract void onResult(boolean z);

        @Override // java.lang.Runnable
        public void run() {
            onResult(this.mGranted);
        }
    }

    @Override // androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        AppManager.getAppManager().finishActivity(this);
    }

    public void showProgressDialog(Context context, String str) {
        if (this.progressDialog == null) {
            ProgressDialog progressDialog = new ProgressDialog(context);
            this.progressDialog = progressDialog;
            progressDialog.setProgressStyle(0);
        }
        this.progressDialog.setMessage(str);
        this.progressDialog.setCancelable(false);
        this.progressDialog.show();
        new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.base.BaseActivity.3
            @Override // java.lang.Runnable
            public void run() {
                BaseActivity.this.dismissProgressDialog().booleanValue();
            }
        }, 10000L);
    }

    public void showProgressDialog1(Context context, String str) {
        if (this.progressDialog == null) {
            ProgressDialog progressDialog = new ProgressDialog(context);
            this.progressDialog = progressDialog;
            progressDialog.setProgressStyle(0);
        }
        this.progressDialog.setMessage(str);
        this.progressDialog.setCancelable(false);
        this.progressDialog.show();
    }

    public Boolean dismissProgressDialog() {
        try {
            ProgressDialog progressDialog = this.progressDialog;
            if (progressDialog != null && progressDialog.isShowing()) {
                this.progressDialog.dismiss();
                return true;
            }
        } catch (IllegalArgumentException e) {
            e.printStackTrace();
        } catch (Exception e2) {
            e2.printStackTrace();
        }
        return false;
    }
}