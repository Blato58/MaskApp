package cn.com.heaton.shiningmask.ui.activity;

import android.view.LayoutInflater;
import android.view.View;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.databinding.ActivityImageBinding;
import cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment;
import cn.com.heaton.shiningmask.ui.fragment.ImageFragment;
import pub.devrel.easypermissions.EasyPermissions;

/* JADX INFO: loaded from: classes.dex */
public class ImageActivity extends BaseActivity<ActivityImageBinding> implements View.OnClickListener {
    private int curSelect = 0;
    private int flag;
    private FragmentManager fm;
    private FragmentTransaction transaction;

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityImageBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityImageBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivGallery.setOnClickListener(this);
        getBinding().ivAnim.setOnClickListener(this);
        getBinding().ivGallery.setImageResource(R.mipmap.image_diy_unselected);
        getBinding().ivAnim.setImageResource(R.mipmap.image_gallery_selected);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.flag = getIntent().getIntExtra("flag", 0);
        initTab();
    }

    private void initTab() {
        FragmentManager supportFragmentManager = getSupportFragmentManager();
        this.fm = supportFragmentManager;
        FragmentTransaction fragmentTransactionBeginTransaction = supportFragmentManager.beginTransaction();
        this.transaction = fragmentTransactionBeginTransaction;
        if (this.flag == 1) {
            this.curSelect = 1;
            getBinding().ivGallery.setImageResource(R.mipmap.image_diy_selected);
            getBinding().ivAnim.setImageResource(R.mipmap.image_gallery_unselected);
            this.transaction.replace(R.id.fl_change, DiyImageFragment.newInstance());
            this.transaction.commit();
            return;
        }
        fragmentTransactionBeginTransaction.replace(R.id.fl_change, ImageFragment.newInstance(0));
        this.transaction.commit();
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        FragmentManager supportFragmentManager = getSupportFragmentManager();
        this.fm = supportFragmentManager;
        this.transaction = supportFragmentManager.beginTransaction();
        int id = view.getId();
        if (id == R.id.iv_anim) {
            if (this.curSelect == 0) {
                return;
            }
            this.curSelect = 0;
            getBinding().ivGallery.setImageResource(R.mipmap.image_diy_unselected);
            getBinding().ivAnim.setImageResource(R.mipmap.image_gallery_selected);
            this.transaction.replace(R.id.fl_change, ImageFragment.newInstance(0));
            this.transaction.commit();
            return;
        }
        if (id != R.id.iv_gallery || this.curSelect == 1) {
            return;
        }
        this.curSelect = 1;
        getBinding().ivGallery.setImageResource(R.mipmap.image_diy_selected);
        getBinding().ivAnim.setImageResource(R.mipmap.image_gallery_unselected);
        this.transaction.replace(R.id.fl_change, DiyImageFragment.newInstance());
        this.transaction.commit();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    public void onRequestPermissionsResult(int i, String[] strArr, int[] iArr) {
        super.onRequestPermissionsResult(i, strArr, iArr);
        EasyPermissions.onRequestPermissionsResult(i, strArr, iArr, DiyImageFragment.newInstance());
    }
}