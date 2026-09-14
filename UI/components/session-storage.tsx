// src/utils/localStorage.ts

class SessionStorage {
  static setToken(token: string) {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.setItem("t", token);
    }
    catch(e){}
  }

    static setRefreshToken(token: string) {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.setItem("rt", token);
    }
    catch(e){}
  }

  static getToken(): string | null {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return null;

      return localStorage.getItem("t");
    }
    catch(e){
      return null;
    }
    
  }

  static getRefreshToken(): string | null {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return null;

      return localStorage.getItem("rt");
    }
    catch(e){
      return null;
    }
    
  }

  static removeToken() {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.removeItem("t");
    }
    catch(e){}
    
  }

  static setUserId(token: string) {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.setItem("u", token);
    }
    catch(e){}
    
  }

  static getUserId(): string | null {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return null;

      return localStorage.getItem("u");
    }
    catch(e){
      return null;}
    
  }

  static removeUserId() {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.removeItem("u");
    }
    catch(e){}
    
  }

  static setSession(token: string) {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.setItem("s", token);
    }
    catch(e){}
    
  }

  static getSession(): string | null {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return null;

      return localStorage.getItem("s");
    }
    catch(e){
      return null;}
    
  }

  static removeSession() {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.removeItem("s");
    }
    catch(e){}
    
  }

  static setName(name: string) {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.setItem("n", name);
    }
    catch(e){}
    
  }

  static getName(): string | null {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return null;

      return localStorage.getItem("n");
    }
    catch(e){
      return null;}
    
  }

  static removeName() {
    try
    {
      if(!window.localStorage || localStorage == null || localStorage == undefined)
      return;

      localStorage.removeItem("n");
    }
    catch(e){}
    
  }
}

export default SessionStorage;
