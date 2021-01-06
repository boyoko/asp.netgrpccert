# asp.netgrpccert
本文主要是实现

1.gprc 客服端 和服务端 实现双向认证

2.普通的js 可以访问grpc服务， 这里采用httpapi 来包装【以前在go里面没有完全实现，在go里面用gateway后，grprc 客户端 和gateway 都只能用http协议， 不能像本文中 兼容https 和http】
