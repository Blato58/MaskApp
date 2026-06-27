package cn.com.heaton.shiningmask.base;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Context;
import android.os.Bundle;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.fragment.app.Fragment;
import androidx.viewbinding.ViewBinding;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;

/* JADX INFO: loaded from: classes.dex */
public abstract class BaseFragment<T extends ViewBinding> extends Fragment {
    protected static final String TAG = "BaseFragment";
    private T binding;
    public boolean isVisible;
    protected Activity mActivity;
    private View mContentView;
    protected Context mContext;
    private ProgressDialog progressDialog;

    protected abstract T inflateBinding(LayoutInflater layoutInflater, ViewGroup viewGroup);

    protected abstract void initData();

    protected abstract void initView(View view, Bundle bundle);

    @Override // androidx.fragment.app.Fragment
    public View onCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle) {
        T t = (T) inflateBinding(layoutInflater, viewGroup);
        this.binding = t;
        this.mContentView = t.getRoot();
        this.mContext = getContext();
        initView(this.mContentView, bundle);
        return this.mContentView;
    }

    @Override // androidx.fragment.app.Fragment
    public void onActivityCreated(Bundle bundle) {
        super.onActivityCreated(bundle);
        initData();
    }

    @Override // androidx.fragment.app.Fragment
    public void onHiddenChanged(boolean z) {
        super.onHiddenChanged(z);
        this.isVisible = !z;
    }

    @Override // androidx.fragment.app.Fragment
    public void onAttach(Context context) {
        super.onAttach(context);
        this.mActivity = (Activity) context;
    }

    protected T getBinding() {
        return this.binding;
    }

    public Context getMContext() {
        return this.mContext;
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
        new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.base.BaseFragment.1
            @Override // java.lang.Runnable
            public void run() {
                if (BaseFragment.this.dismissProgressDialog().booleanValue()) {
                    ToastUtil.showToast(BaseFragment.this.getResources().getString(R.string.timeout));
                }
            }
        }, 10000L);
    }

    public Boolean dismissProgressDialog() {
        ProgressDialog progressDialog = this.progressDialog;
        if (progressDialog != null && progressDialog.isShowing()) {
            this.progressDialog.dismiss();
            this.progressDialog = null;
            return true;
        }
        return false;
    }

    @Override // androidx.fragment.app.Fragment
    public void onResume() {
        super.onResume();
    }

    @Override // androidx.fragment.app.Fragment
    public void onPause() {
        super.onPause();
    }

    @Override // androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
        this.mContext = null;
        this.mActivity = null;
    }
}