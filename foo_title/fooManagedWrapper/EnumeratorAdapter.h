/*
*  This file is part of foo_title.
*  Copyright 2005 - 2006 Roman Plasil (http://foo-title.sourceforge.net)
*  Copyright 2016 Miha Lepej (https://github.com/LepkoQQ/foo_title)
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*
*  See the file COPYING included with this distribution for more
*  information.
*/

#pragma once

using namespace System;
using namespace System::Collections;

namespace fooManagedWrapper
{

template<typename FoobarType, typename RefManagedType>
public ref class CEnumeratorAdapterBase abstract:
     public Generic::IEnumerator<RefManagedType>,
     public Generic::IEnumerable<RefManagedType> {
protected:
     service_enum_t<FoobarType> *enumerator;

public:
     CEnumeratorAdapterBase() {
          enumerator = new service_enum_t<FoobarType>();
     }

     ~CEnumeratorAdapterBase() {
          delete ( enumerator ); 
          enumerator = NULL;
     }

     virtual void Reset() {
          throw gcnew NotImplementedException();
     }

     virtual bool MoveNext() = 0;

     virtual property RefManagedType Current {
          virtual RefManagedType get() = 0;
     }

     virtual property Object ^Current2 {
          virtual Object ^get() = System::Collections::IEnumerator::Current::get {
               return this->Current::get();
          }
     }


     virtual Generic::IEnumerator<RefManagedType> ^GetEnumerator() = 
          Collections::Generic::IEnumerable<RefManagedType>::GetEnumerator {
               return this;
     }

     virtual Collections::IEnumerator ^GetEnumerator2() =
          Collections::IEnumerable::GetEnumerator {
               return this;
     }

};

template<typename FoobarType, typename ManagedType, typename RefManagedType>
public ref class CEnumeratorAdapter :
     public CEnumeratorAdapterBase<FoobarType, RefManagedType> {
private:
     service_ptr_t<FoobarType> *current;
     
public:

     CEnumeratorAdapter() {
          current = NULL;
     }

     virtual ~CEnumeratorAdapter() {
          delete ( current );
          current = NULL;
     }

     !CEnumeratorAdapter() {
          this->~CEnumeratorAdapter();
     }

     const service_ptr_t<FoobarType> &GetCurrent() {
          return *current;
     }

     virtual bool MoveNext() override {
          if (current == NULL)
               current = new service_ptr_t<FoobarType>();

          return enumerator->next(*current);
     };

     virtual property ManagedType ^Current {
          virtual ManagedType ^get() override = System::Collections::Generic::IEnumerator<ManagedType^>::Current::get {
               if (current == NULL)
                    throw gcnew InvalidOperationException("First call MoveNext before accessing the current element.");

               return gcnew ManagedType(*current);
          }
     }		
};

}
