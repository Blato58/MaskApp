package cn.com.heaton.shiningmask.presenter;

import android.util.Log;
import cn.com.heaton.shiningmask.base.BasePresenter;
import cn.com.heaton.shiningmask.contract.MainContract;

/* JADX INFO: loaded from: classes.dex */
public class MainPresenter extends BasePresenter<MainContract.View> implements MainContract.Presenter {
    @Override // cn.com.heaton.shiningmask.contract.MainContract.Presenter
    public void testGetMpresenter() {
        Log.d("print", "我是P层的引用");
        ((MainContract.View) this.mView).testGetMview();
    }

    @Override // cn.com.heaton.shiningmask.contract.MainContract.Presenter
    public void testDb() {
        this.mDataManager.testDb();
    }

    @Override // cn.com.heaton.shiningmask.contract.MainContract.Presenter
    public void testRequestNetwork() {
        this.mDataManager.testRequestNetwork();
    }

    @Override // cn.com.heaton.shiningmask.contract.MainContract.Presenter
    public void testPreference() {
        this.mDataManager.testPreference();
    }
}