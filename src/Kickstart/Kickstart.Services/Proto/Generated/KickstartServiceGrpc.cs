// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: KickstartService.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Kickstart.Services.Types {
  public static partial class KickstartServiceApi
  {
    static readonly string __ServiceName = "kickstart.services.types.KickstartServiceApi";

    static readonly grpc::Marshaller<global::Kickstart.Services.Types.KickstartSolutionRequest> __Marshaller_kickstart_services_types_KickstartSolutionRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.KickstartSolutionRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.KickstartSolutionResponse> __Marshaller_kickstart_services_types_KickstartSolutionResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.KickstartSolutionResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.ConvertDDLRequest> __Marshaller_kickstart_services_types_ConvertDDLRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.ConvertDDLRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.ConvertDDLResponse> __Marshaller_kickstart_services_types_ConvertDDLResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.ConvertDDLResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.SplitDDLRequest> __Marshaller_kickstart_services_types_SplitDDLRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.SplitDDLRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.SplitDDLResponse> __Marshaller_kickstart_services_types_SplitDDLResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.SplitDDLResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.QueryDatabaseTablesRequest> __Marshaller_kickstart_services_types_QueryDatabaseTablesRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.QueryDatabaseTablesRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Kickstart.Services.Types.QueryDatabaseTablesResponse> __Marshaller_kickstart_services_types_QueryDatabaseTablesResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Kickstart.Services.Types.QueryDatabaseTablesResponse.Parser.ParseFrom);

    static readonly grpc::Method<global::Kickstart.Services.Types.KickstartSolutionRequest, global::Kickstart.Services.Types.KickstartSolutionResponse> __Method_KickstartSolution = new grpc::Method<global::Kickstart.Services.Types.KickstartSolutionRequest, global::Kickstart.Services.Types.KickstartSolutionResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "KickstartSolution",
        __Marshaller_kickstart_services_types_KickstartSolutionRequest,
        __Marshaller_kickstart_services_types_KickstartSolutionResponse);

    static readonly grpc::Method<global::Kickstart.Services.Types.ConvertDDLRequest, global::Kickstart.Services.Types.ConvertDDLResponse> __Method_ConvertDDL = new grpc::Method<global::Kickstart.Services.Types.ConvertDDLRequest, global::Kickstart.Services.Types.ConvertDDLResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ConvertDDL",
        __Marshaller_kickstart_services_types_ConvertDDLRequest,
        __Marshaller_kickstart_services_types_ConvertDDLResponse);

    static readonly grpc::Method<global::Kickstart.Services.Types.SplitDDLRequest, global::Kickstart.Services.Types.SplitDDLResponse> __Method_SplitDDL = new grpc::Method<global::Kickstart.Services.Types.SplitDDLRequest, global::Kickstart.Services.Types.SplitDDLResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SplitDDL",
        __Marshaller_kickstart_services_types_SplitDDLRequest,
        __Marshaller_kickstart_services_types_SplitDDLResponse);

    static readonly grpc::Method<global::Kickstart.Services.Types.QueryDatabaseTablesRequest, global::Kickstart.Services.Types.QueryDatabaseTablesResponse> __Method_QueryDatabaseTables = new grpc::Method<global::Kickstart.Services.Types.QueryDatabaseTablesRequest, global::Kickstart.Services.Types.QueryDatabaseTablesResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "QueryDatabaseTables",
        __Marshaller_kickstart_services_types_QueryDatabaseTablesRequest,
        __Marshaller_kickstart_services_types_QueryDatabaseTablesResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Kickstart.Services.Types.KickstartServiceReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of KickstartServiceApi</summary>
    public abstract partial class KickstartServiceApiBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Kickstart.Services.Types.KickstartSolutionResponse> KickstartSolution(global::Kickstart.Services.Types.KickstartSolutionRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Kickstart.Services.Types.ConvertDDLResponse> ConvertDDL(global::Kickstart.Services.Types.ConvertDDLRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Kickstart.Services.Types.SplitDDLResponse> SplitDDL(global::Kickstart.Services.Types.SplitDDLRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Kickstart.Services.Types.QueryDatabaseTablesResponse> QueryDatabaseTables(global::Kickstart.Services.Types.QueryDatabaseTablesRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for KickstartServiceApi</summary>
    public partial class KickstartServiceApiClient : grpc::ClientBase<KickstartServiceApiClient>
    {
      /// <summary>Creates a new client for KickstartServiceApi</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public KickstartServiceApiClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for KickstartServiceApi that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public KickstartServiceApiClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected KickstartServiceApiClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected KickstartServiceApiClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Kickstart.Services.Types.KickstartSolutionResponse KickstartSolution(global::Kickstart.Services.Types.KickstartSolutionRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return KickstartSolution(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Kickstart.Services.Types.KickstartSolutionResponse KickstartSolution(global::Kickstart.Services.Types.KickstartSolutionRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_KickstartSolution, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.KickstartSolutionResponse> KickstartSolutionAsync(global::Kickstart.Services.Types.KickstartSolutionRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return KickstartSolutionAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.KickstartSolutionResponse> KickstartSolutionAsync(global::Kickstart.Services.Types.KickstartSolutionRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_KickstartSolution, null, options, request);
      }
      public virtual global::Kickstart.Services.Types.ConvertDDLResponse ConvertDDL(global::Kickstart.Services.Types.ConvertDDLRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ConvertDDL(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Kickstart.Services.Types.ConvertDDLResponse ConvertDDL(global::Kickstart.Services.Types.ConvertDDLRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ConvertDDL, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.ConvertDDLResponse> ConvertDDLAsync(global::Kickstart.Services.Types.ConvertDDLRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ConvertDDLAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.ConvertDDLResponse> ConvertDDLAsync(global::Kickstart.Services.Types.ConvertDDLRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ConvertDDL, null, options, request);
      }
      public virtual global::Kickstart.Services.Types.SplitDDLResponse SplitDDL(global::Kickstart.Services.Types.SplitDDLRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SplitDDL(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Kickstart.Services.Types.SplitDDLResponse SplitDDL(global::Kickstart.Services.Types.SplitDDLRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SplitDDL, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.SplitDDLResponse> SplitDDLAsync(global::Kickstart.Services.Types.SplitDDLRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SplitDDLAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.SplitDDLResponse> SplitDDLAsync(global::Kickstart.Services.Types.SplitDDLRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SplitDDL, null, options, request);
      }
      public virtual global::Kickstart.Services.Types.QueryDatabaseTablesResponse QueryDatabaseTables(global::Kickstart.Services.Types.QueryDatabaseTablesRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return QueryDatabaseTables(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Kickstart.Services.Types.QueryDatabaseTablesResponse QueryDatabaseTables(global::Kickstart.Services.Types.QueryDatabaseTablesRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_QueryDatabaseTables, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.QueryDatabaseTablesResponse> QueryDatabaseTablesAsync(global::Kickstart.Services.Types.QueryDatabaseTablesRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return QueryDatabaseTablesAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Kickstart.Services.Types.QueryDatabaseTablesResponse> QueryDatabaseTablesAsync(global::Kickstart.Services.Types.QueryDatabaseTablesRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_QueryDatabaseTables, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override KickstartServiceApiClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new KickstartServiceApiClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(KickstartServiceApiBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_KickstartSolution, serviceImpl.KickstartSolution)
          .AddMethod(__Method_ConvertDDL, serviceImpl.ConvertDDL)
          .AddMethod(__Method_SplitDDL, serviceImpl.SplitDDL)
          .AddMethod(__Method_QueryDatabaseTables, serviceImpl.QueryDatabaseTables).Build();
    }

    /// <summary>Register service method implementations with a service binder. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, KickstartServiceApiBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_KickstartSolution, serviceImpl.KickstartSolution);
      serviceBinder.AddMethod(__Method_ConvertDDL, serviceImpl.ConvertDDL);
      serviceBinder.AddMethod(__Method_SplitDDL, serviceImpl.SplitDDL);
      serviceBinder.AddMethod(__Method_QueryDatabaseTables, serviceImpl.QueryDatabaseTables);
    }

  }
}
#endregion
