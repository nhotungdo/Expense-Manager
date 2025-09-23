package com.expense.manager;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;

@Controller
public class HelloController {

    @RequestMapping("/")
    public String índex() {
        return "index";
    }
    
    @RequestMapping("/hello")
    @ResponseBody
    public String hello() {
        return "Hello Tùng nho";
    }

}
