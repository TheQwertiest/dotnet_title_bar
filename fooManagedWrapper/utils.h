/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
#pragma once
#include "Stdafx.h"
#include "FileInfo.h"

using namespace System;
using namespace System::Collections;

namespace fooManagedWrapper
{
	template<typename T> void safe_delete(T &ptr) {
		if (ptr != NULL) {
			delete ptr;
			ptr = NULL;
		}
	}

#define SAFE_DELETE(p) {if ((p) != NULL) { delete (p); (p) = NULL;};}

	// a managed wrapper for metadb_handle
	public ref class CMetaDBHandle 
	{
	public:
		CMetaDBHandle(const metadb_handle_ptr &src);
		!CMetaDBHandle();
		~CMetaDBHandle() { this->!CMetaDBHandle(); };

		metadb_handle_ptr GetHandle() { return *handle; };
		String ^GetPath();
		double GetLength();
	private:
		// metadb_handle_ptr is a smart pointer which handles reference counting, so another level of indirection
		// is rather cumbersome, but it is required by C++/CLI
		metadb_handle_ptr *handle;
	};

	// a managed wrapper for foobar's console
	public ref class CConsole {
	public:
		static void Error(String ^a);
		static void Write(String ^a);
		static void Warning(String ^a);

	};

	public interface class IPlayControl {
		enum class StopReason {
			stop_reason_user = 0,
			stop_reason_eof = 1,
			stop_reason_starting_another = 2,
			stop_reason_shutting_down = 3
		};
		String ^FormatTitle(CMetaDBHandle ^dbHandle, String ^spec);
		CMetaDBHandle ^GetNowPlaying();
		double PlaybackGetPosition();
		bool IsPlaying();
	};

	public ref class CPlayControl : public IPlayControl {
	public:
		virtual CMetaDBHandle ^GetNowPlaying();
		virtual String ^FormatTitle(CMetaDBHandle ^dbHandle, String ^spec);
		virtual double PlaybackGetPosition();
		virtual bool IsPlaying();
	};


	// this is the main entry point for each dotnet_ component - one class must implement it
	public interface class IComponentClient {
		
		// the component class must create all services in this method
		void Create();

		// this also gives the component an IPlayControl implementation
		void OnInit(IPlayControl ^a);
		void OnQuit();

		// these are the play callbacks
		void OnPlaybackNewTrack(CMetaDBHandle ^h);
		void OnPlaybackTime(double time);
		void OnPlaybackPause(bool state);
		void OnPlaybackStop(IPlayControl::StopReason reason);
		void OnPlaybackDynamicInfoTrack(FileInfo ^fileInfo);
	};

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
			SAFE_DELETE(enumerator);
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
			SAFE_DELETE(current);
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

	public enum class Context {
		NowPlaying,
		Playlist
	};
}
