package cn.com.heaton.shiningmask.base;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.widget.TextView;
import android.widget.Toast;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public abstract class BaseDialog extends Dialog {
    public Activity mActivity;
    private AlertDialog mConnectDialog;
    private TextView message;

    protected abstract int getLayoutResource();

    protected void initLinsenter() {
    }

    protected abstract void onInitData();

    protected abstract void onInitView();

    public BaseDialog(Context context) {
        super(context);
        this.mActivity = (Activity) context;
    }

    public BaseDialog(Context context, int i) {
        super(context, i);
        Activity activity = (Activity) context;
        setOwnerActivity(activity);
        this.mActivity = activity;
    }

    @Override // android.app.Dialog
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setCanceledOnTouchOutside(true);
        setContentView(View.inflate(this.mActivity, getLayoutResource(), null));
        Window window = getWindow();
        window.clearFlags(201326592);
        window.getDecorView().setSystemUiVisibility(1792);
        window.addFlags(Integer.MIN_VALUE);
        if (getWindow() != null) {
            getWindow().setLayout(-1, -1);
        }
        onInitView();
        onInitData();
        initLinsenter();
    }

    public void showDialog(String str) {
        if (this.mConnectDialog == null) {
            AlertDialog alertDialogCreate = new AlertDialog.Builder(this.mActivity).setCancelable(false).create();
            this.mConnectDialog = alertDialogCreate;
            alertDialogCreate.setCanceledOnTouchOutside(false);
            this.mConnectDialog.show();
            this.mConnectDialog.setContentView(R.layout.progress_dialog);
            this.message = (TextView) this.mConnectDialog.findViewById(R.id.message);
        }
        this.message.setText(str);
        this.mConnectDialog.show();
    }

    public void dismissDialog() {
        AlertDialog alertDialog = this.mConnectDialog;
        if (alertDialog == null || !alertDialog.isShowing()) {
            return;
        }
        this.mConnectDialog.dismiss();
    }

    public void toast(int i) {
        Toast.makeText(this.mActivity, i, 0).show();
    }
}