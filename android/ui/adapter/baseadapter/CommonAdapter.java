package cn.com.heaton.shiningmask.ui.adapter.baseadapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public abstract class CommonAdapter<T> extends BaseAdapter {
    private GetPos getPos;
    protected Context mContext;
    protected List<T> mDatas;
    protected LayoutInflater mInflater;
    protected final int mItemLayoutId;
    private SetListener setListener;

    public interface GetPos {
        void getPos(int i);
    }

    public interface SetListener {
        void onLastPosition(ViewHolder viewHolder);

        void onOtherPosition(ViewHolder viewHolder);
    }

    public abstract void convert(ViewHolder viewHolder, T t, int i);

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public CommonAdapter(Context context, List<T> list, int i) {
        this.mContext = context;
        this.mInflater = LayoutInflater.from(context);
        this.mDatas = list;
        this.mItemLayoutId = i;
    }

    public CommonAdapter(Context context, int i) {
        this.mContext = context;
        this.mInflater = LayoutInflater.from(context);
        this.mItemLayoutId = i;
        this.mDatas = new ArrayList();
    }

    public void refreshDatas(List<T> list) {
        List<T> list2 = this.mDatas;
        if (list2 != null) {
            list2.clear();
        }
        if (list != null && !list.isEmpty()) {
            this.mDatas.addAll(list);
        }
        notifyDataSetChanged();
    }

    public void clearDatas() {
        this.mDatas.clear();
        notifyDataSetChanged();
    }

    public void removeData(int i) {
        this.mDatas.remove(i);
        notifyDataSetChanged();
    }

    public void addData(T t) {
        this.mDatas.add(t);
        notifyDataSetChanged();
    }

    public void moveData(int i, int i2) {
        this.mDatas.add(i2, this.mDatas.remove(i));
        notifyDataSetChanged();
    }

    public void addMoreDatas(List<T> list) {
        if (list != null && !list.isEmpty()) {
            this.mDatas.addAll(list);
        }
        notifyDataSetChanged();
    }

    @Override // android.widget.Adapter
    public int getCount() {
        return this.mDatas.size();
    }

    @Override // android.widget.Adapter
    public T getItem(int i) {
        List<T> list = this.mDatas;
        if (list != null) {
            return list.get(i);
        }
        return null;
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        ViewHolder viewHolder = getViewHolder(i, view, viewGroup);
        GetPos getPos = this.getPos;
        if (getPos != null) {
            getPos.getPos(i);
        }
        convert(viewHolder, getItem(i), i);
        if (this.setListener != null) {
            if (i == this.mDatas.size() - 1) {
                this.setListener.onLastPosition(viewHolder);
            } else {
                this.setListener.onOtherPosition(viewHolder);
            }
        }
        return viewHolder.getConvertView();
    }

    public void setListener(SetListener setListener) {
        this.setListener = setListener;
    }

    public void addAll(Collection<? extends T> collection) {
        this.mDatas.clear();
        this.mDatas.addAll(collection);
        notifyDataSetChanged();
    }

    public void setGetPosListener(GetPos getPos) {
        this.getPos = getPos;
    }

    private ViewHolder getViewHolder(int i, View view, ViewGroup viewGroup) {
        return ViewHolder.get(this.mContext, view, viewGroup, this.mItemLayoutId, i);
    }

    public List<T> getList() {
        return this.mDatas;
    }
}