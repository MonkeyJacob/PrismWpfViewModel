# PrismWpfViewModel

#### This project is a set of ViewModel base class designed for WPF , and the whole project is based on Prism. 
In this project, view model is considered as a box for Model instance, and also a adapter between Model and View.  

From the "box" perspective, 
the ViewModel should allow replacement for the content of the "box" (which means the Model instance), 
but the "box" itself is still the same instance. 
The advantage of this approach is View avoids some complex operations and code-behind of changing DataContext,
if View has already been established some conection with ViewModel instance.
