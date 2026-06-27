package cn.com.heaton.shiningmask.ui.fragment;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseFragment;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.databinding.FragmentImageBinding;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class ImageFragment extends BaseFragment<FragmentImageBinding> implements View.OnClickListener {
    private static int flag;
    private static ImageFragment fragment;
    private FragmentManager fm;
    private FragmentTransaction transaction;

    public static ImageFragment newInstance(int i) {
        if (fragment == null) {
            flag = i;
            fragment = new ImageFragment();
        }
        return fragment;
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    public FragmentImageBinding inflateBinding(LayoutInflater layoutInflater, ViewGroup viewGroup) {
        return FragmentImageBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initView(View view, Bundle bundle) {
        getBinding().ivDefaultImage.setOnClickListener(this);
        getBinding().ivDiyImage.setOnClickListener(this);
        getBinding().top.ivBack.setOnClickListener(this);
        getBinding().top.ivForward.setOnClickListener(this);
        getBinding().ivLoop.setOnClickListener(this);
        getBinding().ivDefaultImage.setImageResource(R.mipmap.image_anim_unselected);
        getBinding().ivDiyImage.setImageResource(R.mipmap.iamge_top_selected);
        getBinding().top.ivBack.setImageResource(R.mipmap.text_back);
        getBinding().top.ivBack.setVisibility(0);
        getBinding().top.ivForward.setVisibility(8);
        getBinding().top.tvTitle.setText(getString(R.string.gallery));
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initData() {
        initTab();
    }

    private void initTab() {
        FragmentManager childFragmentManager = getChildFragmentManager();
        this.fm = childFragmentManager;
        FragmentTransaction fragmentTransactionBeginTransaction = childFragmentManager.beginTransaction();
        this.transaction = fragmentTransactionBeginTransaction;
        if (flag == 0) {
            selectAnimImage();
        } else {
            fragmentTransactionBeginTransaction.replace(R.id.fl_change, DefaultImageFragment.newInstance());
            this.transaction.commit();
        }
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        FragmentManager childFragmentManager = getChildFragmentManager();
        this.fm = childFragmentManager;
        this.transaction = childFragmentManager.beginTransaction();
        int id = view.getId();
        if (id == R.id.iv_default_image) {
            selectAnimImage();
            return;
        }
        if (id == R.id.iv_diy_image) {
            getBinding().ivLoop.setVisibility(8);
            getBinding().ivDefaultImage.setImageResource(R.mipmap.image_anim_unselected);
            getBinding().ivDiyImage.setImageResource(R.mipmap.iamge_top_selected);
            this.transaction.replace(R.id.fl_change, DefaultImageFragment.newInstance());
            this.transaction.commit();
            return;
        }
        if (id == R.id.iv_loop) {
            EventBus.getDefault().post(C.MAIN_EVENT.UPDATE_ANIM);
        } else if (id == R.id.iv_back) {
            getActivity().finish();
        }
    }

    private void selectAnimImage() {
        getBinding().ivDefaultImage.setImageResource(R.mipmap.image_anim_selected);
        getBinding().ivDiyImage.setImageResource(R.mipmap.iamge_top_unselected);
        getBinding().ivLoop.setVisibility(0);
        this.transaction.replace(R.id.fl_change, AnimFragment.newInstance(true));
        this.transaction.commit();
    }
}