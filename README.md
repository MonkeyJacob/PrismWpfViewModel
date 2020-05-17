# PrismWpfViewModel   

## Introduction  

This project is a set of ViewModel base class designed for WPF , and the whole project is based on Prism.    

In this project, VeiwModel is considered as __a box for Model instance__, and also __a adapter between Model and View__.   

From the "box" perspective, the ViewModel should allow not only intial packing but also replacement of the content of the "box" (which means the Model instance), but the "box" itself is still the same instance. 
When a packing or replacement of the Model instance occurs, the VieModel should raise NotifyPropertyChanged event for all properties which are bound to View,
and if the Model instance is enumerable for some sub-object, the ViewModel should clear old sub-ViewModel_object manufacture the sub-ViewModel-object automatically.
The advantage of this mechanism is View avoids some complex operations and C# code (code-behind) of changing DataContext, if View has already been established some connection with ViewModel instance.
The entrance of this mechanism is ModelObject.set().    

From the "adapter" perspective.