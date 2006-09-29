#pragma once
#include "Stdafx.h"

using namespace System;

namespace fooManagedWrapper
{
	// a managed wrapper for metadb_handle
	public ref class MetaDBHandle 
	{
	public:
		MetaDBHandle(metadb_handle *src);
		!MetaDBHandle();
		~MetaDBHandle();

		metadb_handle *GetHandle() { return handle; };
		String ^GetPath();
	private:
		metadb_handle *handle;
	};

	// a managed wrapper for foobar's console
	public ref class Console {
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
		String ^FormatTitle(MetaDBHandle ^dbHandle, String ^spec);
	};

	public ref class CPlayControl : public IPlayControl {
	public:
	
		virtual String ^FormatTitle(MetaDBHandle ^dbHandle, String ^spec);
	};


	// this is the main entry point for each dotnet_ component - one class must implement it
	public interface class IComponentClient {
		
		// the component class must create all services in this method
		void Create();

		// this also gives the component an IPlayControl implementation
		void OnInit(IPlayControl ^a);
		void OnQuit();

		// these are the play callbacks
		void OnPlaybackNewTrack(MetaDBHandle ^h);
		void OnPlaybackTime(double time);
		void OnPlaybackPause(bool state);
		void OnPlaybackStop(IPlayControl::StopReason reason);
	};

}
