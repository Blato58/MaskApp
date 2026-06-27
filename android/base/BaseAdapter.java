package cn.com.heaton.shiningmask.base;

import android.content.Context;
import android.util.SparseArray;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public abstract class BaseAdapter<T> extends android.widget.BaseAdapter {
    protected Context mContext;
    private List<T> mList;

    public abstract int getContentView();

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public abstract void onInitView(View view, int i);

    public BaseAdapter(Context context) {
        init(context, new ArrayList());
    }

    public BaseAdapter(Context context, List<T> list) {
        init(context, list);
    }

    private void init(Context context, List<T> list) {
        this.mList = list;
        this.mContext = context;
    }

    public List<T> getList() {
        return this.mList;
    }

    public void setList(List<T> list) {
        this.mList = list;
    }

    public void clear() {
        this.mList.clear();
        notifyDataSetChanged();
    }

    public void addAll(List<T> list) {
        if (list != null) {
            this.mList.addAll(list);
            notifyDataSetChanged();
        }
    }

    @Override // android.widget.Adapter
    public int getCount() {
        List<T> list = this.mList;
        if (list == null) {
            return 0;
        }
        return list.size();
    }

    @Override // android.widget.Adapter
    public T getItem(int i) {
        return this.mList.get(i);
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        if (view == null) {
            view = inflate(getContentView());
        }
        onInitView(view, i);
        return view;
    }

    private View inflate(int i) {
        return ((LayoutInflater) this.mContext.getSystemService("layout_inflater")).inflate(i, (ViewGroup) null);
    }

    /* JADX WARN: Incorrect return type in method signature: <T:Landroid/view/View;>(Landroid/view/View;I)TT; */
    public static View getAdapterView(View view, int i) {
        SparseArray sparseArray = (SparseArray) view.getTag();
        if (sparseArray == null) {
            sparseArray = new SparseArray();
            view.setTag(sparseArray);
        }
        View view2 = (View) sparseArray.get(i);
        if (view2 != null) {
            return view2;
        }
        View viewFindViewById = view.findViewById(i);
        sparseArray.put(i, viewFindViewById);
        return viewFindViewById;
    }
}