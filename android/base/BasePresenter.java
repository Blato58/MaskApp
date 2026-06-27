package cn.com.heaton.shiningmask.base;

import cn.com.heaton.shiningmask.base.IBaseView;
import cn.com.heaton.shiningmask.model.db.DbHelper;
import cn.com.heaton.shiningmask.model.http.ApiHelper;
import cn.com.heaton.shiningmask.model.preference.PreferenceHelper;

/* JADX INFO: loaded from: classes.dex */
public abstract class BasePresenter<V extends IBaseView> implements IBasePresenter<V> {
    protected cn.com.heaton.shiningmask.model.DataManager mDataManager = new cn.com.heaton.shiningmask.model.DataManager(new DbHelper(), new ApiHelper(), new PreferenceHelper());
    protected V mView;

    @Override // cn.com.heaton.shiningmask.base.IBasePresenter
    public void attachView(V v) {
        this.mView = v;
    }
}