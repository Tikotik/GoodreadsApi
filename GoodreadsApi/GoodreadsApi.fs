﻿module GoodreadsApi

open OAuth
open Parser

let requestTokenUrl = "http://www.goodreads.com/oauth/request_token"
let accessTokenUrl = "http://www.goodreads.com/oauth/access_token"
let authorizeUrl = sprintf "https://www.goodreads.com/oauth/authorize?oauth_token=%s&oauth_callback=%s"

let getAuthorizationData clientKey clientSecret callbackUrl =
    let (token, tokenSecret) = getAuthorizatonToken clientKey clientSecret requestTokenUrl
    (authorizeUrl token callbackUrl, token, tokenSecret) 

let getAccessToken clientKey clientSecret token tokenSecret =
    getAccessToken clientKey clientSecret accessTokenUrl token tokenSecret

let getAccessData clientKey clientSecret token tokenSecret =
    getAccessData clientKey clientSecret token tokenSecret
        
let getUser accessData = 
    let userResponse = getUrlContentWithAccessData "https://www.goodreads.com/api/auth_user" accessData

    parseUser userResponse

let getReviewsOnPage accessData userId shelf sort perPage pageNumber = 
    let url = 
        sprintf "https://www.goodreads.com/review/list/%i.xml?key=%s&v=2&shelf=%s&sort=%s&per_page=%i&page=%i" userId accessData.ClientKey 
            shelf sort perPage pageNumber
    let reviewsResponse = getUrlContentWithAccessData url accessData
    parseReviews reviewsResponse

let getAllReviews accessData userId shelf sort = 
    let perPage = 200
    let getReviewsOnPage = getReviewsOnPage accessData userId shelf sort perPage
    let firstPageReviews = getReviewsOnPage 1
    
    let pagesCount = 
        float firstPageReviews.Total / float perPage
        |> ceil
        |> int
    
    let first = seq firstPageReviews.Reviews
    
    let others = 
        [ 2..pagesCount ]
        |> Seq.map getReviewsOnPage
        |> Seq.collect (fun pageReviews -> pageReviews.Reviews)
    Seq.concat [| first; others |]

let getBookDetail accessData bookId=
    let url = 
        sprintf "https://www.goodreads.com/book/show/%i.xml?key=%s" bookId accessData.ClientKey 
    let bookDetailResponse = getUrlContentWithAccessData url accessData
    parseBookDetail bookDetailResponse