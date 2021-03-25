import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { retry, catchError } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { TemperatureData } from '../models/temperature-data';

@Injectable({
  providedIn: 'root'
})
export class TemperatureDataService {
  myAppUrl: string;
  myApiUrl: string;
  httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8'
    })
  };

  constructor(private http: HttpClient) {
    this.myAppUrl = environment.appUrl;
    this.myApiUrl = 'api/TemperatureDatas/';
  }

  getLast20Data(): Observable<TemperatureData[]> {
    return this.http.get<TemperatureData[]>(this.myAppUrl + this.myApiUrl + "last20")
      .pipe(
        retry(1),
        catchError(this.errorHandler)
      );
  }

  getLast(): Observable<TemperatureData[]> {
    return this.http.get<TemperatureData[]>(this.myAppUrl + this.myApiUrl + "last")
      .pipe(
        retry(1),
        catchError(this.errorHandler)
      );
  }

  deleteData(dataId: number): Observable<TemperatureData> {
    return this.http.delete<TemperatureData>(this.myAppUrl + this.myApiUrl + dataId)
      .pipe(
        retry(1),
        catchError(this.errorHandler)
      );
  }

  errorHandler(error) {
    let errorMessage = '';
    if (error.error instanceof ErrorEvent) {
      // Get client-side error
      errorMessage = error.error.message;
    } else {
      // Get server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    console.log(errorMessage);
    return throwError(errorMessage);
  }
}