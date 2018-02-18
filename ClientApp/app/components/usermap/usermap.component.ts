import { Component, Inject } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';

@Component({
    selector: 'usermap',
    templateUrl: './usermap.component.html'
})
export class UserMapComponent {
    private http: Http;
    private baseUrl: string;
    public reposts: Categorie[];
    public likes: Categorie[];
    public isRepostLoading = false;
    public isLikeLoading = false;
    public isUserLoading = false;
    public userName: string = "";
    public user: User;
    public period = 365;

    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.http = http;
        this.baseUrl = baseUrl;
    }

    public getUser() {
        this.isUserLoading = true;
        
        this.http.get(this.baseUrl + 'api/SocialData/person/' + this.userName).subscribe(
            result => {
                this.isUserLoading = false;
                this.user = result.json() as User;
                this.getUserMap();
                this.getUserLikes(this.user.id);
            },
            error => {
                this.isUserLoading = false;
                console.error(error);
            }
        );
    }
    
    public getUserMap() {
        this.isRepostLoading = true;
        this.reposts = [];

        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({   headers: headers });

        this.http.post(
            this.baseUrl + 'api/SocialData/repost',
            { user: this.userName, period: this.period },
            options)
            .subscribe(
                result => {
                    this.isRepostLoading = false;
                    this.reposts = result.json() as Categorie[];
                },
                error => {
                    this.isRepostLoading = false;
                    console.error(error);
                });

    }

    private getUserLikes(userId: number) {
        this.isLikeLoading = true;
        this.likes = [];
        this.http.get(this.baseUrl + 'api/SocialData/like/' + userId).subscribe(
            result => {
                this.isLikeLoading = false;
                this.likes = result.json() as Categorie[];
            },
            error => {
                this.isLikeLoading = false;
                console.error(error);
            }
        );
    }
}

interface Categorie {
    name: string;
    actionCount: number;
}

interface User {
    id: number;
    lastName: string;
    firstName: string;
    birthDate: string;
}
